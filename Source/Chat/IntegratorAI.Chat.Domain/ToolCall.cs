namespace IntegratorAI.Chat.Domain;

/// <summary>A request of the model to run a tool. <see cref="Arguments"/> is a JSON object.</summary>
public record ToolCall(string Id, string Name, string Arguments);
