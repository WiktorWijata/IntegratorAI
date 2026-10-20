using System.Text.Json.Serialization;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>Null on an assistant message that only asks for tools.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ChatToolCall[]? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolCallId { get; set; }
}
