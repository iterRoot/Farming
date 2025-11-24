
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using AutoMapper;
using FarmingApi.Core;
// using FarmingApi.Modules.Items;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// builder.Services.AddAutoMapper(typeof(ItemsMapper)); // Only register once

builder.Services.AddOpenApi();
builder.Services.AddDbContext<MyDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Registers the generic repository
// builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddInjection();
builder.Services.AddDatabase();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
        app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapGet("/", () => builder.Environment);
app.MapControllers();
app.Run();
