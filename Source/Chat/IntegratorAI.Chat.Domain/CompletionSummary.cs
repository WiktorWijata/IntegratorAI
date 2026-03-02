namespace IntegratorAI.Chat.Domain;

public class CompletionSummary
{
    public Guid CompletionId { get; set; }
    public string Content { get; protected set; }
    public int SummarizedUpToIndex { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? ModifiedAt { get; protected set; }

    public CompletionSummary(string content, int summarizedUpToIndex)
    {
        Content = content;
        SummarizedUpToIndex = summarizedUpToIndex;
        CreatedAt = DateTime.UtcNow;
    }

    protected CompletionSummary()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string content, int summarizedUpToIndex)
    {
        Content = content;
        SummarizedUpToIndex = summarizedUpToIndex;
        ModifiedAt = DateTime.UtcNow;
    }
}
