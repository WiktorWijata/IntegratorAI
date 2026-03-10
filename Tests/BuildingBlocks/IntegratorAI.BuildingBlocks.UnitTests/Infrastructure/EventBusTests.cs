using IntegratorAI.BuildingBlocks.Common.Models.Events;
using IntegratorAI.BuildingBlocks.Domain.Event;
using IntegratorAI.BuildingBlocks.Infrastructure.Event;
using NSubstitute;

namespace IntegratorAI.BuildingBlocks.UnitTests.Infrastructure;

public class EventBusTests
{
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

    private void SetupHandlers(params IHandleEvent<TestEvent>[] handlers)
    {
        _serviceProvider
            .GetService(typeof(IEnumerable<IHandleEvent<TestEvent>>))
            .Returns(handlers);
    }

    [Fact]
    public async Task PublishAsync_InvokesAllRegisteredHandlers()
    {
        var handler1 = Substitute.For<IHandleEvent<TestEvent>>();
        var handler2 = Substitute.For<IHandleEvent<TestEvent>>();
        handler1.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        handler2.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        SetupHandlers(handler1, handler2);

        var eventBus = new EventBus(_serviceProvider);
        var @event = new TestEvent();

        await eventBus.PublishAsync(@event);

        await handler1.Received(1).Handle(@event, Arg.Any<CancellationToken>());
        await handler2.Received(1).Handle(@event, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_DoesNotThrow_WhenNoHandlersRegistered()
    {
        SetupHandlers();

        var eventBus = new EventBus(_serviceProvider);

        var exception = await Record.ExceptionAsync(() => eventBus.PublishAsync(new TestEvent()));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_PassesCancellationToken_ToHandlers()
    {
        var handler = Substitute.For<IHandleEvent<TestEvent>>();
        handler.Handle(Arg.Any<TestEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        SetupHandlers(handler);

        var eventBus = new EventBus(_serviceProvider);
        using var cts = new CancellationTokenSource();

        await eventBus.PublishAsync(new TestEvent(), cts.Token);

        await handler.Received(1).Handle(Arg.Any<TestEvent>(), cts.Token);
    }
}
