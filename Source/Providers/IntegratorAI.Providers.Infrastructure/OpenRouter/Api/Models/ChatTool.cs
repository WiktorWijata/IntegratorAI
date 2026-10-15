using System.Text.Json.Serialization;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;

public class ChatTool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public required ChatFunctionDefinition Function { get; set; }
}

public class ChatFunctionDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>A JSON Schema of the arguments.</summary>
    [JsonPropertyName("parameters")]
    public required object Parameters { get; set; }
}
