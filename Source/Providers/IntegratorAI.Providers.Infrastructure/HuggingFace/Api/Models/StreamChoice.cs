using System.Text.Json.Serialization;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;

public class StreamChoice
{
    [JsonPropertyName("delta")]
    public StreamDelta? Delta { get; set; }
}
