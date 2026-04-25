using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnderworldAPI.Purchases.Application.Abstractions;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Purchases.Infrastructure.Persistence;

public sealed class PurchasesDbContext(
    DbContextOptions<PurchasesDbContext> options,
    IPublisher publisher
):DbContext(options),IUnitOfWork
{
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PurchasesDbContext).Assembly);

        // Schema separado — misma DB, distinto schema
        modelBuilder.HasDefaultSchema("purchases");

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.GetDomainEvents().Any())
            .SelectMany(a =>
            {
                var events = a.GetDomainEvents();
                a.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}
