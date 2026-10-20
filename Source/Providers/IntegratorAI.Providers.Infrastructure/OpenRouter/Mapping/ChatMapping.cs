using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Requests;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Mapping;

public static class ChatMapping
{
    private const string ToolChoiceAuto = "auto";

    extension(ProviderCompletionDto dto)
    {
        public ChatRequest ToChatRequest(string model, bool stream = false)
        {
            var tools = dto.Tools is { Length: > 0 } ? dto.Tools.Select(ToChatTool).ToArray() : null;

            return new ChatRequest
            {
                Model = model,
                Stream = stream,
                Messages = dto.Messages.Select(ToChatMessage).ToArray(),
                Tools = tools,
                ToolChoice = tools is null ? null : ToolChoiceAuto,
            };
        }
    }

    extension(ChatMessage message)
    {
        public ProviderMessageDto ToMessageDto()
        {
            return new ProviderMessageDto
            {
                Role = message.Role,
                Content = message.Content,
            };
        }
    }

    private static ChatMessage ToChatMessage(ProviderMessageDto message)
    {
        var hasToolCalls = message.ToolCalls is { Length: > 0 };

        return new ChatMessage
        {
            Role = message.Role?.ToLowerInvariant(),
            Content = hasToolCalls && string.IsNullOrEmpty(message.Content) ? null : message.Content,
            ToolCalls = hasToolCalls
                ? message.ToolCalls!.Select(c => new ChatToolCall
                {
                    Id = c.Id,
                    Type = "function",
                    Function = new ChatFunctionCall { Name = c.Name, Arguments = c.Arguments },
                }).ToArray()
                : null,
            ToolCallId = message.ToolCallId,
        };
    }

    private static ChatTool ToChatTool(ProviderToolDto tool)
    {
        var parameters = tool.Parameters ?? [];

        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = parameters.ToDictionary(
                p => p.Name,
                p => (object)new Dictionary<string, object>
                {
                    ["type"] = ToJsonSchemaType(p.Type),
                    ["description"] = p.Description ?? string.Empty,
                }),
            ["required"] = parameters.Where(p => p.Required).Select(p => p.Name).ToArray(),
        };

        return new ChatTool
        {
            Function = new ChatFunctionDefinition { Name = tool.Name, Description = tool.Description, Parameters = schema },
        };
    }

    /// <summary>The type names of the tools are the C# ones; JSON Schema knows string, integer, number and boolean.</summary>
    private static string ToJsonSchemaType(string? type) => type?.ToLowerInvariant() switch
    {
        "int" or "int32" or "long" or "int64" or "integer" => "integer",
        "double" or "decimal" or "float" or "number" => "number",
        "bool" or "boolean" => "boolean",
        _ => "string",
    };
}
