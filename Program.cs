
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using AutoMapper;
using FarmingApi.Core;
using FarmingApi.Modules.SaleAR.ARInvoice;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.Financials.GLAccountDetermination;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;

// using FarmingApi.Modules.Items;
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options =>
{
    // Enforces the None/Read/Full permission map the Authorisations screen
    // already edits (KUAU) — see Core/SubjectAuthorizationFilter.cs. Runs
    // after authentication, so it only ever narrows what an already-logged-in
    // user can do.
    options.Filters.Add<FarmingApi.Core.SubjectAuthorizationFilter>();
    // Wires the PostingPeriods status dropdown to actual enforcement — see
    // Core/PostingPeriodGuardFilter.cs. Runs after the auth filters above,
    // so it only ever narrows what an already-permitted user can post.
    options.Filters.Add<FarmingApi.Core.PostingPeriodGuardFilter>();
    options.Filters.Add<FarmingApi.Core.EmployeeTagFilter>();
    options.Filters.Add<FarmingApi.Core.DocLinkFilter>();
    options.Filters.Add<FarmingApi.Modules.Administration.UserDefinedFields.UdfCleanupFilter>();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            // Vite picks the next free port when 5173 is already taken
            // (5174, 5175, ...) — accept any localhost/127.0.0.1 port
            // instead of hardcoding one, so a stray dev server doesn't
            // get silently CORS-blocked.
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                    return uri.Host is "localhost" or "127.0.0.1";
                })
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
        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description  = "Paste the token from POST /User/Login (without the word Bearer).",
        };
        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer",
                },
            }] = Array.Empty<string>(),
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
builder.Services.AddScoped<FarmingApi.Modules.Administration.Attachment.AttachmentStorage>();
builder.Services.AddScoped<FarmingApi.Modules.Financials.GLAccountDetermination.GLAccountResolver>();
builder.Services.AddHttpClient().ConfigureHttpClientDefaults(b => b.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseProxy = builder.Configuration.GetValue("ExchangeRates:UseProxy", true) }));
builder.Services.AddScoped<FarmingApi.Modules.Financials.ExchangeRate.ExchangeRateFetcher>();
builder.Services.AddHostedService<FarmingApi.Modules.Financials.ExchangeRate.ExchangeRateDailyJob>();
builder.Services.AddInjection();
builder.Services.AddDatabase();

// Bearer-token auth. The signing key comes from user-secrets (Development)
// or the Jwt__Key environment variable — see JwtTokenService.GetSigningKey.
// Login (POST /User/Login) issues the token; everything else validates it.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "FarmingApi";
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FarmingApiClients";
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
    });

// Monthly payroll run. Creates DRAFT payslips only; never touches a Paid one.
// Schedule + on/off live under "Payroll:AutoRun" in appsettings.json.
builder.Services.AddHostedService<FarmingApi.Modules.HumanResources.Payroll.PayrollAutoRunWorker>();
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
var app = builder.Build();

// Opt-in (RunMigrationsOnStartup=true): hosted deploys start from an empty database.
if (builder.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var migrationScope = app.Services.CreateScope();
    migrationScope.ServiceProvider.GetRequiredService<MyDbContext>().Database.Migrate();
}

// Guarantee a "Manager" login always exists — idempotent, safe to run
// on every startup (fresh database, restore, or accidental deletion).
FarmingApi.Core.DefaultUserSeeder.Seed(app.Services);

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
        options.SwaggerEndpoint("/openapi/v1.json", "Farming API v1");
        options.RoutePrefix = "swagger"; // Serves the UI at localhost:5181/swagger
        options.EnablePersistAuthorization(); // keep the pasted token across page reloads
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");   // 👈 MUST be BEFORE auth/authorization
app.UseAuthentication();     // 👈 MUST be BEFORE UseAuthorization
app.UseAuthorization();
app.MapGet("/", () => builder.Environment);
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok());

app.MapControllers();
app.Run();
