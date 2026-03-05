using IntegratorAI.BuildingBlocks.Common.Models.Events;
using IntegratorAI.BuildingBlocks.Domain.Event;
using IntegratorAI.BuildingBlocks.Infrastructure.Event;
using Moq;

namespace IntegratorAI.BuildingBlocks.UnitTests.Infrastructure;

public class EventBusTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();

    private void SetupHandlers(params IHandleEvent<TestEvent>[] handlers)
    {
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IHandleEvent<TestEvent>>)))
            .Returns(handlers);
    }

    [Fact]
    public async Task PublishAsync_InvokesAllRegisteredHandlers()
    {
        var handlerMock1 = new Mock<IHandleEvent<TestEvent>>();
        var handlerMock2 = new Mock<IHandleEvent<TestEvent>>();
        handlerMock1.Setup(h => h.Handle(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        handlerMock2.Setup(h => h.Handle(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        SetupHandlers(handlerMock1.Object, handlerMock2.Object);

        var eventBus = new EventBus(_serviceProviderMock.Object);
        var @event = new TestEvent();

        await eventBus.PublishAsync(@event);

        handlerMock1.Verify(h => h.Handle(@event, It.IsAny<CancellationToken>()), Times.Once());
        handlerMock2.Verify(h => h.Handle(@event, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task PublishAsync_DoesNotThrow_WhenNoHandlersRegistered()
    {
        SetupHandlers();

        var eventBus = new EventBus(_serviceProviderMock.Object);

        var exception = await Record.ExceptionAsync(() => eventBus.PublishAsync(new TestEvent()));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_PassesCancellationToken_ToHandlers()
    {
        var handlerMock = new Mock<IHandleEvent<TestEvent>>();
        handlerMock.Setup(h => h.Handle(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        SetupHandlers(handlerMock.Object);

        var eventBus = new EventBus(_serviceProviderMock.Object);
        using var cts = new CancellationTokenSource();

        await eventBus.PublishAsync(new TestEvent(), cts.Token);

        handlerMock.Verify(h => h.Handle(It.IsAny<TestEvent>(), cts.Token), Times.Once());
    }
}
