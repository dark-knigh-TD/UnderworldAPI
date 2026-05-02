using System;
using UnderworldAPI.Shared.Domain.Primitives;

namespace UnderworldAPI.Sales.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
