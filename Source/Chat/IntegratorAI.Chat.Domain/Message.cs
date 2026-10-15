using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace IntegratorAI.Chat.Domain;

public class Message
{
    public int Id { get; set; }
    public Guid CompletionId { get; set; }
    public int Index { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    /// <summary>The tools an assistant message asked for instead of (or besides) answering, as JSON.</summary>
    public string? ToolCallsJson { get; protected set; }

    /// <summary>The call a tool message answers.</summary>
    public string? ToolCallId { get; protected set; }

    [NotMapped]
    public IReadOnlyList<ToolCall> ToolCalls
        => ToolCallsJson is null ? [] : JsonSerializer.Deserialize<List<ToolCall>>(ToolCallsJson) ?? [];

    public Message(MessageRole role, string content)
    {
        Role = role;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    protected Message()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>An assistant message that asks for tools. <paramref name="content"/> is the text said before asking, usually empty.</summary>
    public static Message ForToolCalls(string content, IEnumerable<ToolCall> toolCalls)
        => new(MessageRole.Assistant, content) { ToolCallsJson = JsonSerializer.Serialize(toolCalls) };

    /// <summary>The answer to one tool call.</summary>
    public static Message ForToolResult(string toolCallId, string content)
        => new(MessageRole.Tool, content) { ToolCallId = toolCallId };
}
