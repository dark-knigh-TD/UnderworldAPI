using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Sales.Infrastructure.Persistence;

public sealed class SalesDbContext(
    DbContextOptions<SalesDbContext> options,
    IPublisher publisher
):DbContext(options),IUnitOfWork
{
     public DbSet<Order> Orders => Set<Order>();

      protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas las IEntityTypeConfiguration del assembly automáticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);

        // Todos los objetos de este contexto van al schema [sales]
        modelBuilder.HasDefaultSchema("sales");

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Recolectar todos los domain events pendientes de los aggregates tracked
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

        // Persistir primero
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publicar eventos después de persistir — si la DB falla, no se publican
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        return result;
    }

}
