using UnderworldAPI.Sales.API.DependencyInjection;
using UnderworldAPI.Sales.API.Middleware;
using UnderworldAPI.Sales.Infrastructure.Persistence;
using UnderworldAPI.Sales.Infrastructure.DependencyInjection;
using UnderworldAPI.Sales.Application.DependencyInjection;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

/// ── Azure Key Vault ───────────────────────────────────────────
// Orden de prioridad:
// 1. Variable de entorno AZURE_KEYVAULT_URI (launch.json en F5, Container App en Azure)
// 2. AzureKeyVault:Uri desde appsettings.json (fallback, normalmente vacío)
// Si ninguno está configurado, se omite Key Vault — los secrets vienen de appsettings.Development.json
//
// Por ambiente:
// - Local F5        → launch.json inyecta AZURE_KEYVAULT_URI → az login (DefaultAzureCredential)
// - docker-compose  → No usa Key Vault — secrets vienen del .env y docker-compose.override.yml
// - Azure (Prod)    → Container App inyecta AZURE_KEYVAULT_URI → Managed Identity (DefaultAzureCredential)
var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URI")
    ?? builder.Configuration["AzureKeyVault:Uri"]; //ya lee de appsettings.development.json, pero se puede sobreescribir con variable de entorno para más flexibilidad
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCredential = true,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeAzureCliCredential = false
        }));
}

// ── Services ──────────────────────────────────────────────────
builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── JWT — se agrega después de Key Vault para que lea JwtSecretKey ──
builder.Services.AddJwtAuthentication(builder.Configuration);

// Health checks — requerido por Azure Container Apps para saber si el contenedor está vivo
builder.Services.AddHealthChecks();
    //.AddDbContextCheck<SalesDbContext>("sales-db");
    
// builder.Services.AddControllers();
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

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
app.UseAuthentication(); //  antes de Authorization
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
