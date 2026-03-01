namespace IntegratorAI.Chat.Domain;

public class Completion
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Message> Messages { get; protected set; } = new List<Message>();

    public Completion()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void AddMessage(MessageRole role, string content)
    {
        Messages.Add(new Message
        {
            CompletionId = Id,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        });
    }
}
