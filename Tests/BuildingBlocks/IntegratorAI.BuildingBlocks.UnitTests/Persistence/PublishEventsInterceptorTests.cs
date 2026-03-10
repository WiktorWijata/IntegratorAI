using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using IntegratorAI.BuildingBlocks.Common.Models.Domain;
using IntegratorAI.BuildingBlocks.Common.Models.Events;
using IntegratorAI.BuildingBlocks.Domain.Event;
using IntegratorAI.BuildingBlocks.Persistence;
using NSubstitute;

namespace IntegratorAI.BuildingBlocks.UnitTests.Persistence;

public class PublishEventsInterceptorTests
{
    private class InterceptorTestDbContext : DbContext
    {
        public DbSet<TestAggregateRoot> AggregateRoots { get; set; } = null!;

        public InterceptorTestDbContext(DbContextOptions options) : base(options) { }
    }

    private static DbContextOptions CreateOptions() =>
        new DbContextOptionsBuilder<InterceptorTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static SaveChangesCompletedEventData CreateEventData(DbContext? context) =>
        new(null!, (_, _) => string.Empty, context!, 1);

    private static IEventBus CreateEventBusMock()
    {
        var mock = Substitute.For<IEventBus>();
        mock.PublishAsync(Arg.Any<IEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        return mock;
    }

    [Fact]
    public async Task SavedChangesAsync_PublishesAllEventsFromAggregateRoots()
    {
        using var context = new InterceptorTestDbContext(CreateOptions());
        var aggregate = new TestAggregateRoot();
        aggregate.AddEvent(new TestEvent());
        aggregate.AddEvent(new TestEvent());
        context.Add(aggregate);

        var eventBusMock = CreateEventBusMock();
        var interceptor = new PublishEventsInterceptor(eventBusMock);

        await interceptor.SavedChangesAsync(CreateEventData(context), 1);

        await eventBusMock.Received(2).PublishAsync(Arg.Any<IEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavedChangesAsync_ClearsDomainEventsAfterPublish()
    {
        using var context = new InterceptorTestDbContext(CreateOptions());
        var aggregate = new TestAggregateRoot();
        aggregate.AddEvent(new TestEvent());
        context.Add(aggregate);

        var interceptor = new PublishEventsInterceptor(CreateEventBusMock());

        await interceptor.SavedChangesAsync(CreateEventData(context), 1);

        Assert.Empty(aggregate.GetDomainEvents());
    }

    [Fact]
    public async Task SavedChangesAsync_DoesNotPublish_WhenNoAggregateRootsTracked()
    {
        using var context = new InterceptorTestDbContext(CreateOptions());
        var eventBusMock = CreateEventBusMock();
        var interceptor = new PublishEventsInterceptor(eventBusMock);

        await interceptor.SavedChangesAsync(CreateEventData(context), 0);

        await eventBusMock.DidNotReceive().PublishAsync(Arg.Any<IEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavedChangesAsync_DoesNotPublish_WhenAggregateHasNoEvents()
    {
        using var context = new InterceptorTestDbContext(CreateOptions());
        var aggregate = new TestAggregateRoot();
        context.Add(aggregate);

        var eventBusMock = CreateEventBusMock();
        var interceptor = new PublishEventsInterceptor(eventBusMock);

        await interceptor.SavedChangesAsync(CreateEventData(context), 1);

        await eventBusMock.DidNotReceive().PublishAsync(Arg.Any<IEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavedChangesAsync_DoesNotThrow_WhenContextIsNull()
    {
        var eventBusMock = CreateEventBusMock();
        var interceptor = new PublishEventsInterceptor(eventBusMock);

        var exception = await Record.ExceptionAsync(() =>
            interceptor.SavedChangesAsync(CreateEventData(null), 0).AsTask());

        Assert.Null(exception);
        await eventBusMock.DidNotReceive().PublishAsync(Arg.Any<IEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavedChangesAsync_PassesCancellationToken_ToEventBus()
    {
        using var context = new InterceptorTestDbContext(CreateOptions());
        var aggregate = new TestAggregateRoot();
        aggregate.AddEvent(new TestEvent());
        context.Add(aggregate);

        var eventBusMock = CreateEventBusMock();
        var interceptor = new PublishEventsInterceptor(eventBusMock);
        using var cts = new CancellationTokenSource();

        await interceptor.SavedChangesAsync(CreateEventData(context), 1, cts.Token);

        await eventBusMock.Received(1).PublishAsync(Arg.Any<IEvent>(), cts.Token);
    }
}
