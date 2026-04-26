using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UnderworldAPI.Purchases.API.DependencyInjection;
using UnderworldAPI.Purchases.API.Middleware;
using UnderworldAPI.Purchases.Application.DependencyInjection;
using UnderworldAPI.Purchases.Infrastructure.DependencyInjection;
using UnderworldAPI.Purchases.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);


// ── Services ──────────────────────────────────────────────────
builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// builder.Services.AddControllers();
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PurchasesDbContext>("purchases-db");
    
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "UnderworldAPI — Purchases";
        options.Theme = ScalarTheme.DeepSpace;
    });// /scalar/v1  ← UI visual
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PurchasesDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
