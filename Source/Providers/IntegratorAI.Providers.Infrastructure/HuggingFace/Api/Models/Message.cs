namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;

public class Message
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}
