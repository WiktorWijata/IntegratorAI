using IntegratorAI.BuildingBlocks.Domain.Event;

namespace IntegratorAI.BuildingBlocks.Domain;

public interface IAggregateRoot
{
    IReadOnlyCollection<IEvent> GetDomainEvents();
    void ClearDomainEvents();
}
