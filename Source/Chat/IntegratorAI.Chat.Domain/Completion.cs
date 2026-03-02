using IntegratorAI.BuildingBlocks.Domain;
using IntegratorAI.Chat.Domain.Events;

namespace IntegratorAI.Chat.Domain;

public class Completion : AggregateRoot<Guid>
{
    public override Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public CompletionSummary? Summary { get; protected set; }
    public IEnumerable<Message> UnsummarizedMessages => Messages.Where(m => m.Index > (Summary?.SummarizedUpToIndex ?? 0));
    public ICollection<Message> Messages { get; protected set; } = new List<Message>();

    public Completion(Message message)
    {
        CreatedAt = DateTime.UtcNow;
        AddMessage(message);
    }

    protected Completion()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void AddMessage(Message message)
    {
        message.Index = Messages.Count + 1;
        Messages.Add(message);
    }

    public bool TrySummarize(int threshold)
    {
        if (UnsummarizedMessages.Count() <= threshold)
        {
            return false;
        }

        RaiseEvent(new CompletionSummaryRequiredEvent(this));
        return true;
    }

    public void SetSummary(string content, int summarizedUpToIndex)
    {
        if (Summary is null)
        {
            Summary = new CompletionSummary(content, summarizedUpToIndex);
        }
        else
        {
            Summary.Update(content, summarizedUpToIndex);
        }
    }
}
