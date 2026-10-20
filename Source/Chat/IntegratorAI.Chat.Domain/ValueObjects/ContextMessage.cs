namespace IntegratorAI.Chat.Domain.ValueObjects;

public record ContextMessage(MessageRole Role, string Content, string? ToolCallsJson = null, string? ToolCallId = null);
