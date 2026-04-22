using System;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Sales.Domain.Events;

public sealed record OrderCreatedEvent(
Guid OrderId,
    Guid CustomerId,
    DateTime OccurredAt
) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}