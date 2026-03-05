using IntegratorAI.BuildingBlocks.Domain.Event;

namespace IntegratorAI.BuildingBlocks.Common.Models.Events;

public class TestEvent : IEvent
{
    public int TimesInvoked { get; set; }
}

