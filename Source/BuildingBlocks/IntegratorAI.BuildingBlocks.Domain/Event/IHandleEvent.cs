namespace IntegratorAI.BuildingBlocks.Domain.Event;

public interface IHandleEvent { }

public interface IHandleEvent<in TEvent> : IHandleEvent where TEvent : IEvent
{
    Task Handle(TEvent @event, CancellationToken cancellationToken = default);
}
