using IntegratorAI.Chat.Domain.Events;
using IntegratorAI.Chat.Domain.ValueObjects;
using RescuePC.Software.Domain;

namespace IntegratorAI.Chat.Domain;

public class Completion : AggregateRoot<Guid>
{
    public override Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public Guid? ContextId { get; protected set; }
    public CompletionSummary? Summary { get; protected set; }
    public IEnumerable<Message> UnsummarizedMessages => Messages.Where(m => m.Index > (Summary?.SummarizedUpToIndex - 1 ?? 0));
    public IEnumerable<ContextMessage> ContextMessages => GetContextMessages();
    public ICollection<Message> Messages { get; protected set; } = new List<Message>();

    /// <summary>The tools the model asked for in its last message and that have not been answered yet.</summary>
    public IReadOnlyList<ToolCall> PendingToolCalls
    {
        get
        {
            var last = Messages.OrderBy(m => m.Index).LastOrDefault();
            return last is { Role: MessageRole.Assistant, ToolCallsJson: not null } ? last.ToolCalls : [];
        }
    }

    public Completion(Message message, Message? systemMessage = null, Guid? contextId = null)
    {
        CreatedAt = DateTime.UtcNow;
        ContextId = contextId;
        if (systemMessage is not null)
        {
            AddMessage(systemMessage);
        }
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

    /// <summary>Answers every pending tool call at once; the results must be exactly for the calls the model asked for.</summary>
    public void AddToolResults(IReadOnlyDictionary<string, string> contentByCallId)
    {
        var pending = PendingToolCalls;

        if (pending.Count == 0)
        {
            throw new InvalidOperationException("The completion is not waiting for tool results.");
        }

        if (contentByCallId.Count != pending.Count || pending.Any(call => !contentByCallId.ContainsKey(call.Id)))
        {
            throw new InvalidOperationException("The tool results do not match the tool calls the model asked for.");
        }

        foreach (var call in pending)
        {
            AddMessage(Message.ForToolResult(call.Id, contentByCallId[call.Id]));
        }
    }

    public bool TrySummarize(int threshold)
    {
        // A summary must not cut between a tool call and its answer.
        if (PendingToolCalls.Count > 0)
        {
            return false;
        }

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

    private IEnumerable<ContextMessage> GetContextMessages()
    {
        var firstMessage = Messages.OrderBy(m => m.Index).FirstOrDefault();
        if (firstMessage?.Role == MessageRole.System)
        {
            yield return new ContextMessage(firstMessage.Role, firstMessage.Content);
        }

        if (Summary is not null)
        {
            yield return new ContextMessage(MessageRole.System, $"Summary of the conversation so far: {Summary.Content}");
        }

        foreach (var message in UnsummarizedMessages)
        {
            if (message == firstMessage)
            {
                continue;
            }

            yield return new ContextMessage(message.Role, message.Content, message.ToolCallsJson, message.ToolCallId);
        }
    }
}
