using System.Text.Json.Serialization;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;

public class StreamDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
