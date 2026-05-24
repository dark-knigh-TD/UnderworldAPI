using System;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Sales.Infrastructure.Auth;
using UnderworldAPI.Sales.Infrastructure.Messaging;
using UnderworldAPI.Sales.Infrastructure.Persistence;
using UnderworldAPI.Sales.Infrastructure.Persistence.Repositories;
using UnderworldAPI.Shared.Domain.Auth;

namespace UnderworldAPI.Sales.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Aquí registraríamos servicios específicos de la infraestructura, como repositorios, clientes de mensajería, etc.
        // Por ejemplo:
        // services.AddScoped<IOrderRepository, OrderRepository>();
        // services.AddSingleton<IEventPublisher, AzureServiceBusPublisher>();

        // En desarrollo lee de appsettings.Development.json
        // En producción lee de Key Vault automáticamente
        var salesDbConnection = configuration["SalesDbConnectionString"]
        ?? configuration.GetConnectionString("SalesDb");

        // ── EF Core — SQL Server ──────────────────────────────
        services.AddDbContext<SalesDbContext>(options =>
            options.UseSqlServer(
                salesDbConnection,
                sqlOptions =>
                {
                    // Retry automático en fallos transitorios de Azure SQL
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    // Timeout de migración
                    sqlOptions.CommandTimeout(60);
                }));

        // ── IUnitOfWork → SalesDbContext ──────────────────────
        // SalesDbContext implementa IUnitOfWork — misma instancia por request (Scoped)
        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<SalesDbContext>());

        // ── Repositories ──────────────────────────────────────
        services.AddScoped<IOrderRepository, OrderRepository>();

        // ── Azure Service Bus ─────────────────────────────────
        var serviceBusConnection = configuration["ServiceBusConnectionString"]
            ?? configuration["AzureServiceBus:ConnectionString"];

        services.AddSingleton(sp =>
            new ServiceBusClient(serviceBusConnection));

        services.AddScoped<IEventPublisher, AzureServiceBusPublisher>();

        // ── Auth ──────────────────────────────────────────────────────
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

        // ── Integration Events ────────────────────────────────
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();


        return services;
    }
}
