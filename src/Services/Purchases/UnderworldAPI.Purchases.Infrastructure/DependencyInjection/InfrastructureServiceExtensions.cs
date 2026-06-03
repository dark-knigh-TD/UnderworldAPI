using System;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnderworldAPI.Purchases.Application.Abstractions;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Purchases.Infrastructure.Messaging;
using UnderworldAPI.Purchases.Infrastructure.Persistence;
using UnderworldAPI.Purchases.Infrastructure.Persistence.Repositories;
using UnderworldAPI.Sales.Infrastructure.Auth;
using UnderworldAPI.Shared.Domain.Auth;

namespace UnderworldAPI.Purchases.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        var purchasesDbConnection = configuration["PurchasesDbConnectionString"]
        ?? configuration.GetConnectionString("PurchasesDb");

        // ── EF Core — SQL Server ──────────────────────────────
        services.AddDbContext<PurchasesDbContext>(options =>
            options.UseSqlServer(
                purchasesDbConnection,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    sqlOptions.CommandTimeout(60);
                }));

        // ── IUnitOfWork → PurchasesDbContext ──────────────────
        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<PurchasesDbContext>());

        // ── Repositories ──────────────────────────────────────
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();

        // ── Azure Service Bus ─────────────────────────────────
        var serviceBusConnection = configuration["ServiceBusConnectionString"]
            ?? configuration["AzureServiceBus:ConnectionString"];


// TEMPORAL — debug
Console.WriteLine($"[DEBUG] ServiceBusConnectionString: '{configuration["ServiceBusConnectionString"]}'");
Console.WriteLine($"[DEBUG] AzureServiceBus:ConnectionString: '{configuration["AzureServiceBus:ConnectionString"]}'");
Console.WriteLine($"[DEBUG] Final value: '{serviceBusConnection}'");

        services.AddSingleton(sp =>
            new ServiceBusClient(serviceBusConnection));

        services.AddScoped<IEventPublisher, AzureServiceBusPublisher>();

        // ── Auth ──────────────────────────────────────────────────────
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

          // ── Background Service — Consumer de sales-events ─────
        // Singleton porque BackgroundService debe ser Singleton
        services.AddHostedService<SalesEventsConsumer>();

        return services;
    }
}
