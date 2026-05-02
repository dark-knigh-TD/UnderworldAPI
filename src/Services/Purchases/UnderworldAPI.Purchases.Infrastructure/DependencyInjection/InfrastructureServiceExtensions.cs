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

namespace UnderworldAPI.Purchases.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core — SQL Server ──────────────────────────────
        services.AddDbContext<PurchasesDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("PurchasesDb"),
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
        services.AddSingleton(sp =>
            new ServiceBusClient(configuration["AzureServiceBus:ConnectionString"]));

        services.AddScoped<IEventPublisher, AzureServiceBusPublisher>();

        return services;
    }
}
