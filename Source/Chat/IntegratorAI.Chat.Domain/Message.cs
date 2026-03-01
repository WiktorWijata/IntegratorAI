namespace IntegratorAI.Chat.Domain;

public class Message
{
    public int Id { get; set; }
    public Guid CompletionId { get; set; }
    public MessageRole Role { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
