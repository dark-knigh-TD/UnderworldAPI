using UnderworldAPI.Sales.API.DependencyInjection;
using UnderworldAPI.Sales.API.Middleware;
using UnderworldAPI.Sales.Infrastructure.Persistence;
using UnderworldAPI.Sales.Infrastructure.DependencyInjection;
using UnderworldAPI.Sales.Application.DependencyInjection;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────
builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);


// Health checks — requerido por Azure Container Apps para saber si el contenedor está vivo
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SalesDbContext>("sales-db");
    
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────
// El orden importa — ExceptionHandling debe ir primero para capturar todo
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar UI en /scalar/v1 — reemplazo moderno de Swagger UI
    app.MapScalarApiReference(options =>
    {
        options.Title = "UnderworldAPI — Sales";
        options.Theme = ScalarTheme.DeepSpace;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint — Azure Container Apps lo llama periódicamente
app.MapHealthChecks("/health");

// Aplicar migrations automáticamente al arrancar — útil en desarrollo
// En producción esto se maneja con el pipeline de CI/CD
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    await dbContext.Database.MigrateAsync();
}
app.Run();
