using System.Text.Json.Serialization;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Responses;

public class ChatStreamChunk
{
    [JsonPropertyName("choices")]
    public ChatStreamChoice[]? Choices { get; set; }
}

public class ChatStreamChoice
{
    [JsonPropertyName("delta")]
    public ChatStreamDelta? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class ChatStreamDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    public ChatToolCall[]? ToolCalls { get; set; }
}
