using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;

public class MessageRequest
{
    public required Message[] Messages { get; set; }
    public required string Model { get; set; }
    public bool Stream { get; set; } = false;
}
