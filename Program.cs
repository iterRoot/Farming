
using Microsoft.EntityFrameworkCore;
using KamnttraApi;
using AutoMapper;
using KamnttraApi.Core;
using KamnttraApi.Modules.SaleAR.ARInvoice;
using KamnttraApi.Modules.Financials.JournalEntry;
using KamnttraApi.Modules.Financials.GLAccountDetermination;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;

// using KamnttraApi.Modules.Items;
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);   // don't announce the server software
builder.Services.AddControllers(options =>
{
    // Enforces the None/Read/Full permission map the Authorisations screen
    // already edits (KUAU) — see Core/SubjectAuthorizationFilter.cs. Runs
    // after authentication, so it only ever narrows what an already-logged-in
    // user can do.
    options.Filters.Add<KamnttraApi.Core.SubjectAuthorizationFilter>();
    // Wires the PostingPeriods status dropdown to actual enforcement — see
    // Core/PostingPeriodGuardFilter.cs. Runs after the auth filters above,
    // so it only ever narrows what an already-permitted user can post.
    options.Filters.Add<KamnttraApi.Core.PostingPeriodGuardFilter>();
    options.Filters.Add<KamnttraApi.Core.EmployeeTagFilter>();
    options.Filters.Add<KamnttraApi.Core.DocLinkFilter>();
    options.Filters.Add<KamnttraApi.Core.DocumentRuleFilter>();   // DocumentRuleException → 400 with its message
    options.Filters.Add<KamnttraApi.Modules.Administration.UserDefinedFields.UdfCleanupFilter>();
});
// Deployed front-end origins (comma-separated), e.g. Cors__AllowedOrigins=https://kamnttra.vercel.app
var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(o => o.TrimEnd('/'))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);
var allowLocalhost = builder.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Cors:AllowLocalhost");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            // Vite picks the next free port when 5179 is already taken
            // (5174, 5175, ...) — accept any localhost/127.0.0.1 port
            // instead of hardcoding one, so a stray dev server doesn't
            // get silently CORS-blocked.
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                    // Local dev servers are only trusted in Development (or with Cors__AllowLocalhost=true).
                    // In production any page running on someone's localhost shouldn't be treated as the app.
                    var local = (uri.Host is "localhost" or "127.0.0.1") && allowLocalhost;
                    return local || allowedOrigins.Contains(origin.TrimEnd('/'));
                })
                .WithExposedHeaders("X-Access-Token", "X-Access-Token-Expires")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
// builder.Services.AddAutoMapper(typeof(ItemsMapper)); // Only register once

builder.Services.AddOpenApi(options =>
{
    // Every endpoint except /User/Login needs a JWT: describe that in the spec so
    // Swagger UI shows an "Authorize" button and sends the token with each call.
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "Paste the token from POST /User/Login (without the word Bearer).",
        };
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
        });
        return Task.CompletedTask;
    });
});
builder.Services.AddDbContext<MyDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Registers the generic repository
// builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddScoped<IARInvoiceJournalService, ARInvoiceJournalService>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
builder.Services.AddScoped<IGLAccountDeterminationRepository, GLAccountDeterminationRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<KamnttraApi.Core.SecurityAudit>();   // security event log + alerts (Core/SecurityAudit.cs)
builder.Services.AddScoped<KamnttraApi.Modules.Administration.Attachment.AttachmentStorage>();
builder.Services.AddScoped<KamnttraApi.Modules.Financials.GLAccountDetermination.GLAccountResolver>();
builder.Services.AddHttpClient().ConfigureHttpClientDefaults(b => b.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseProxy = builder.Configuration.GetValue("ExchangeRates:UseProxy", true) }));
builder.Services.AddScoped<KamnttraApi.Modules.Financials.ExchangeRate.ExchangeRateFetcher>();
builder.Services.AddHostedService<KamnttraApi.Modules.Financials.ExchangeRate.ExchangeRateDailyJob>();
builder.Services.AddInjection();
builder.Services.AddDatabase();

// Bearer-token auth. The signing key comes from user-secrets (Development)
// or the Jwt__Key environment variable — see JwtTokenService.GetSigningKey.
// Login (POST /User/Login) issues the token; everything else validates it.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "KamnttraApi";
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "KamnttraApiClients";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience             = jwtAudience,
            IssuerSigningKey          = JwtTokenService.GetSigningKey(builder.Configuration),
            ClockSkew                 = TimeSpan.FromMinutes(1),
        };
        // A token that was sent but failed validation (expired, forged, wrong key) is a security event.
        // The reason is the exception type only — never the token itself.
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.HttpContext.RequestServices.GetRequiredService<KamnttraApi.Core.SecurityAudit>()
                    .TokenRejected($"{ctx.Exception.GetType().Name} on {ctx.Request.Method} {ctx.Request.Path}");
                return Task.CompletedTask;
            },
            // A valid signature isn't enough: the user must still exist, be active and unlocked, and the token
            // must have been issued after their last sign-out / password change / lock / deactivation.
            OnTokenValidated = async ctx =>
            {
                var db = ctx.HttpContext.RequestServices.GetRequiredService<KamnttraApi.MyDbContext>();
                var id = int.TryParse(ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var i) ? i : 0;
                var u = await db.Set<KamnttraApi.Modules.Administration.SetUp.User.User>().AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => new { x.ActiveStatus, x.IsLocked, x.LockedUntil, x.SessionsValidFrom })
                    .FirstOrDefaultAsync();
                var issuedMs = long.TryParse(ctx.Principal?.FindFirst("iatms")?.Value, out var ms) ? ms : 0;   // tokens from before this check have no iatms
                string? why = null;
                if (u == null) why = "user no longer exists";
                else if (u.ActiveStatus != 'A') why = "user is inactive";
                else if (u.IsLocked && (u.LockedUntil == null || u.LockedUntil.Value.ToUniversalTime() > DateTime.UtcNow)) why = "user is locked";
                else if (u.SessionsValidFrom is DateTime from && issuedMs < new DateTimeOffset(from.ToUniversalTime()).ToUnixTimeMilliseconds())   // the DB layer hands back Local-kind values
                    why = "signed out, password changed or reset since this token was issued";
                // The session as a whole has a ceiling, however active the user is.
                var startedMs = long.TryParse(ctx.Principal?.FindFirst("authms")?.Value, out var st) ? st : 0;
                if (why == null && startedMs > 0 &&
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedMs > TimeSpan.FromHours(KamnttraApi.Core.JwtTokenService.MaxSessionHours(ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>())).TotalMilliseconds)
                    why = "session reached its maximum length";
                if (why == null && ctx.Principal != null && issuedMs > 0 &&
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - issuedMs > TimeSpan.FromMinutes(KamnttraApi.Core.JwtTokenService.RefreshAfterMinutes(ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>())).TotalMilliseconds)
                {
                    // Active user, token getting old: hand back a fresh one (same session) in a response header.
                    var (fresh, freshExpires) = ctx.HttpContext.RequestServices.GetRequiredService<KamnttraApi.Core.IJwtTokenService>().Refresh(ctx.Principal);
                    ctx.Response.OnStarting(() =>
                    {
                        ctx.Response.Headers["X-Access-Token"] = fresh;
                        ctx.Response.Headers["X-Access-Token-Expires"] = freshExpires.ToString("o");
                        return Task.CompletedTask;
                    });
                }
                if (why != null)
                {
                    ctx.HttpContext.RequestServices.GetRequiredService<KamnttraApi.Core.SecurityAudit>()
                        .TokenRejected($"{why} ({ctx.Request.Method} {ctx.Request.Path})");
                    ctx.Fail(why);
                }
            },
        };
    });

// Monthly payroll run. Creates DRAFT payslips only; never touches a Paid one.
// Schedule + on/off live under "Payroll:AutoRun" in appsettings.json.
builder.Services.AddHostedService<KamnttraApi.Modules.HumanResources.Payroll.PayrollAutoRunWorker>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin.Numbering", p =>
        p.RequireAuthenticatedUser()
            .RequireRole("Administrator"));

    options.AddPolicy("Numbering.Read", p =>
        p.RequireAuthenticatedUser());

    // Default-deny: any action with neither [Authorize] nor [AllowAnonymous]
    // now requires a valid token. Rolled out module by module by removing
    // the blanket [AllowAnonymous] each controller was scaffolded with —
    // an action that still carries it stays open regardless of this policy.
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
// Rate limits. Sign-in is the sensitive one (password guessing): a few attempts per minute per client IP.
// Everything else gets a generous ceiling that only stops floods. Behind Render the client IP comes from
// the forwarded headers (ASPNETCORE_FORWARDEDHEADERS_ENABLED=true).
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = (ctx, _) =>
    {
        ctx.HttpContext.RequestServices.GetRequiredService<KamnttraApi.Core.SecurityAudit>()
            .RateLimited($"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}");
        if (ctx.Lease.TryGetMetadata(System.Threading.RateLimiting.MetadataName.RetryAfter, out var retry))
            ctx.HttpContext.Response.Headers.RetryAfter = ((int)retry.TotalSeconds).ToString();
        return ValueTask.CompletedTask;
    };
    o.AddPolicy("login", http => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        { PermitLimit = builder.Configuration.GetValue<int?>("Security:LoginPerMinute") ?? 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    o.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(http =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            { PermitLimit = builder.Configuration.GetValue<int?>("Security:RequestsPerMinute") ?? 1500, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var app = builder.Build();

// Opt-in (RunMigrationsOnStartup=true): hosted deploys start from an empty database.
if (builder.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var migrationScope = app.Services.CreateScope();
    migrationScope.ServiceProvider.GetRequiredService<MyDbContext>().Database.Migrate();
}

// Guarantee a "Manager" login always exists — idempotent, safe to run
// on every startup (fresh database, restore, or accidental deletion).
KamnttraApi.Core.DefaultUserSeeder.Seed(app.Services);

if (app.Environment.IsDevelopment())
{
    // 1. Generate the OpenAPI JSON document
    // The spec itself is readable without a token (development only), otherwise
    // Swagger UI can't load it: every other endpoint still needs one.
    app.MapOpenApi().AllowAnonymous();
    
    // 2. Serve the Swagger UI and point it to the generated JSON
    // Note: It's 'UseSwaggerUI' (capital UI) and 'SwaggerEndpoint'
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Kamnttra API v1");
        options.RoutePrefix = "swagger"; // Serves the UI at localhost:5182/swagger
        options.EnablePersistAuthorization(); // keep the pasted token across page reloads
    });
}

if (!app.Environment.IsDevelopment()) app.UseHsts();   // only sent on HTTPS responses; Render's proxy supplies the scheme

// Headers for every API response. The API never serves a page, so framing, sniffing and caching are all off.
app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        var h = ctx.Response.Headers;
        h["X-Content-Type-Options"] = "nosniff";
        h["X-Frame-Options"]        = "DENY";
        h["Referrer-Policy"]        = "no-referrer";
        h["Permissions-Policy"]     = "camera=(), microphone=(), geolocation=(), payment=()";
        h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        h["Cross-Origin-Resource-Policy"] = "same-site";
        // Business data must not sit in a shared or back-button cache after sign-out. Skip Swagger in Development.
        if (!ctx.Request.Path.StartsWithSegments("/swagger") && !h.ContainsKey("Cache-Control"))
        {
            h["Cache-Control"] = "no-store";
            h["Pragma"]        = "no-cache";
        }
        return Task.CompletedTask;
    });
    await next();
});
app.UseHttpsRedirection();
app.UseCors("AllowReact");   // 👈 MUST be BEFORE auth/authorization
app.UseRateLimiter();
app.UseAuthentication();     // 👈 MUST be BEFORE UseAuthorization
app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new { status = "ok" }));   // still needs a token; says nothing about the host
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok());

app.MapControllers();
app.Run();
