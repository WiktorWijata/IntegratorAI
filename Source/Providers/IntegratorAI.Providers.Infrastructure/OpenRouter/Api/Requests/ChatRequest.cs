using System.Text.Json.Serialization;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Requests;

public class ChatRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required ChatMessage[] Messages { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChatTool[]? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolChoice { get; set; }
}
