using IntegratorAI.BuildingBlocks.Common.Models.Domain;
using IntegratorAI.BuildingBlocks.Common.Models.Events;
using IntegratorAI.BuildingBlocks.Domain.Event;

namespace IntegratorAI.BuildingBlocks.UnitTests.Domain;

public class AggregateRootTests
{
    [Fact]
    public void RaiseEvent_AddsEventToCollection()
    {
        var aggregate = new TestAggregateRoot();
        var @event = new TestEvent();

        aggregate.AddEvent(@event);

        Assert.Single(aggregate.GetDomainEvents());
        Assert.Contains(@event, aggregate.GetDomainEvents());
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var aggregate = new TestAggregateRoot();
        aggregate.AddEvent(new TestEvent());
        aggregate.AddEvent(new TestEvent());

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.GetDomainEvents());
    }

    [Fact]
    public void GetDomainEvents_ReturnsReadOnlyCollection()
    {
        var aggregate = new TestAggregateRoot();
        aggregate.AddEvent(new TestEvent());

        var result = aggregate.GetDomainEvents();

        Assert.IsAssignableFrom<IReadOnlyCollection<IEvent>>(result);
    }

    [Fact]
    public void MultipleRaiseEvent_PreservesOrder()
    {
        var aggregate = new TestAggregateRoot();
        var first = new TestEvent();
        var second = new TestEvent();
        var third = new TestEvent();

        aggregate.AddEvent(first);
        aggregate.AddEvent(second);
        aggregate.AddEvent(third);

        var events = aggregate.GetDomainEvents().ToList();
        Assert.Equal(first, events[0]);
        Assert.Equal(second, events[1]);
        Assert.Equal(third, events[2]);
    }
}
