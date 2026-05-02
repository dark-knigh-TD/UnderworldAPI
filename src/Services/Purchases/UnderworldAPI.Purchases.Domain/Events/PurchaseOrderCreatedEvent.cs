using System;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Purchases.Domain.Events;

public sealed record PurchaseOrderCreatedEvent(
    Guid PurchaseOrderId, Guid SupplierId, DateTime OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
