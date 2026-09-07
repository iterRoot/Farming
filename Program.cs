
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using AutoMapper;
using FarmingApi.Core;   
using FarmingApi.Modules.SaleAR.ARInvoice;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.Financials.GLAccountDetermination;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;

// using FarmingApi.Modules.Items;
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173") // or 5173 if Vite
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
// builder.Services.AddAutoMapper(typeof(ItemsMapper)); // Only register once

builder.Services.AddOpenApi();
builder.Services.AddDbContext<MyDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Registers the generic repository
// builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddScoped<IARInvoiceJournalService, ARInvoiceJournalService>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
builder.Services.AddScoped<IGLAccountDeterminationRepository, GLAccountDeterminationRepository>();
builder.Services.AddInjection();
builder.Services.AddDatabase();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin.Numbering", p =>
        p.RequireAuthenticatedUser()
            .RequireRole("Administrator"));

    options.AddPolicy("Numbering.Read", p =>
        p.RequireAuthenticatedUser());
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // 1. Generate the OpenAPI JSON document
    app.MapOpenApi(); 
    
    // 2. Serve the Swagger UI and point it to the generated JSON
    // Note: It's 'UseSwaggerUI' (capital UI) and 'SwaggerEndpoint'
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Farming API v1");
        options.RoutePrefix = "swagger"; // Serves the UI at localhost:5181/swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapGet("/", () => builder.Environment);
app.UseCors("AllowReact");   // 👈 MUST be BEFORE MapControllers
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok());

app.MapControllers();
app.Run();
