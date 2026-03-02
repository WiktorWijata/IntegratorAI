using IntegratorAI.BuildingBlocks.Domain.Event;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.BuildingBlocks.Infrastructure.Event;

public class EventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;

    public EventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var handlers = _serviceProvider.GetServices<IHandleEvent<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.Handle(@event, cancellationToken);
        }
    }
}
