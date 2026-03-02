namespace IntegratorAI.Chat.Domain;

public class Completion
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; protected set; }
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
        Messages.Add(message);
    }
}
