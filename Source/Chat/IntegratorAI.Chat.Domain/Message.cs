namespace IntegratorAI.Chat.Domain;

public class Message
{
    public int Id { get; set; }
    public Guid CompletionId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    public Message(MessageRole role, string content)
    {
        Role = role;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    protected Message()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
