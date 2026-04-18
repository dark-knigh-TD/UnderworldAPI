using System;

namespace UnderworldAPI.Shared.Domain.Primitives;

public abstract class AggregateRoot:Entity
{
private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(Guid id) : base(id) { }

    protected AggregateRoot() { }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() =>
        _domainEvents.AsReadOnly();

    public void ClearDomainEvents() =>
        _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);
}
