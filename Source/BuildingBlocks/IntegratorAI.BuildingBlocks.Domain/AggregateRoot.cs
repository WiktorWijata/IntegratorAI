using System.Collections.Generic;
using IntegratorAI.BuildingBlocks.Domain.Event;

namespace IntegratorAI.BuildingBlocks.Domain;

public abstract class AggregateRoot<TKey> : IAggregateRoot
{
    public abstract TKey Id { get; protected set; }

    private readonly List<IEvent> _domainEvents = [];

    protected void RaiseEvent(IEvent @event) => _domainEvents.Add(@event);

    public IReadOnlyCollection<IEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class AggregateRoot : AggregateRoot<long> { }
