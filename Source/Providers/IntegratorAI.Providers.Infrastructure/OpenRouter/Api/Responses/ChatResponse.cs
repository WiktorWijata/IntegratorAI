using System.Text.Json.Serialization;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Responses;

public class ChatResponse
{
    [JsonPropertyName("choices")]
    public ChatChoice[]? Choices { get; set; }
}

public class ChatChoice
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }
}
