using System.Text.Json.Serialization;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;

/// <summary>
/// A request of the model to run a tool. In a stream it arrives in pieces: the first one carries the id and the name,
/// the following ones carry the next part of the arguments, all with the same <see cref="Index"/>.
/// </summary>
public class ChatToolCall
{
    [JsonPropertyName("index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Index { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("function")]
    public ChatFunctionCall? Function { get; set; }
}

public class ChatFunctionCall
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>A JSON object as text.</summary>
    [JsonPropertyName("arguments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Arguments { get; set; }
}
