using System;

namespace UnderworldAPI.Shared.Domain.Primitives;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }

}
