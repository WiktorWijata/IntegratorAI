using IntegratorAI.BuildingBlocks.Common.Models.Events;
using IntegratorAI.BuildingBlocks.Domain.Event;
using IntegratorAI.BuildingBlocks.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.BuildingBlocks.IntegrationTests.Events;

public class EventsIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public EventsIntegrationTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddEventBus(typeof(TestEvent).Assembly);
        _serviceProvider = serviceCollection.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true });
    }

    [Fact]
    public async Task EventBus_PublishAsync_InvokesAllRegisteredHandlers()
    {
        var expectedHandlerCount = typeof(TestEvent).Assembly
            .GetTypes()
            .Count(t => !t.IsAbstract && !t.IsInterface &&
                        t.GetInterfaces().Any(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IHandleEvent<>) &&
                            i.GetGenericArguments()[0] == typeof(TestEvent)));

        using var scope = _serviceProvider.CreateScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        var @event = new TestEvent();

        await eventBus.PublishAsync(@event);

        Assert.Equal(expectedHandlerCount, @event.TimesInvoked);
    }
}
