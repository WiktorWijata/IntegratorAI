using IntegratorAI.BuildingBlocks.Domain;
using IntegratorAI.BuildingBlocks.Domain.Event;

namespace IntegratorAI.BuildingBlocks.Common.Models.Domain;

public class TestAggregateRoot : AggregateRoot
{
    public override long Id { get; protected set; }

    public void AddEvent(IEvent @event) => RaiseEvent(@event);
}
