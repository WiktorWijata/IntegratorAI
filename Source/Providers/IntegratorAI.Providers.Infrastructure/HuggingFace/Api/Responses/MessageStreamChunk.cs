using System.Text.Json.Serialization;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;

public class MessageStreamChunk
{
    [JsonPropertyName("choices")]
    public StreamChoice[]? Choices { get; set; }
}
