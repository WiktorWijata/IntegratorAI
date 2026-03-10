using IntegratorAI.BuildingBlocks.Domain.Event;

namespace IntegratorAI.BuildingBlocks.Common.Models.Events;

public class TestEventHandler : IHandleEvent<TestEvent>
{
    public Task Handle(TestEvent @event, CancellationToken cancellationToken = default)
    {
        @event.TimesInvoked++;
        return Task.CompletedTask;
    }
}