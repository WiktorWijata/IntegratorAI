using IntegratorAI.BuildingBlocks.Domain.Event;

namespace IntegratorAI.Chat.Domain.Events;

public class CompletionSummaryRequiredEvent : IEvent
{
    public CompletionSummaryRequiredEvent(Completion completion)
    {
        Completion = completion;
    }

    public Completion Completion { get; }
}
