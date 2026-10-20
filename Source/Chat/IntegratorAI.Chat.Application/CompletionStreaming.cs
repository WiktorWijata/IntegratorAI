using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Context.Contracts.Models;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;

namespace IntegratorAI.Chat.Application;

/// <summary>What the stream handlers share: what is sent to the provider, and what is kept of its answer.</summary>
internal static class CompletionStreaming
{
    public static ProviderCompletionDto ToProviderCompletion(this Completion completion, IEnumerable<ToolDto>? tools)
    {
        var providerTools = tools?.Select(t => new ProviderToolDto
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = (t.Parameters ?? []).Select(p => new ProviderToolParameterDto
            {
                Name = p.Name,
                Type = p.Type,
                Description = p.Description,
                Required = p.Required,
            }).ToArray(),
        }).ToArray();

        return new ProviderCompletionDto
        {
            Messages = completion.ContextMessages.Select(m => new ProviderMessageDto
            {
                Role = m.Role.ToString(),
                Content = m.Content,
                ToolCallId = m.ToolCallId,
                ToolCalls = m.ToolCallsJson is null
                    ? null
                    : JsonSerializer.Deserialize<List<ToolCall>>(m.ToolCallsJson)!
                        .Select(c => new ProviderToolCallDto { Id = c.Id, Name = c.Name, Arguments = c.Arguments })
                        .ToArray(),
            }).ToArray(),
            Tools = providerTools is { Length: > 0 } ? providerTools : null,
        };
    }

    /// <summary>
    /// Streams the answer of the provider and keeps it in the completion: the text, or the tools the model asked for.
    /// </summary>
    public static async IAsyncEnumerable<CompletionStreamEvent> StreamAnswer(
        this IProviderModule providerModule,
        Completion completion,
        ProviderCompletionDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var text = new StringBuilder();
        var toolCalls = new List<ToolCall>();

        await foreach (var item in providerModule.StreamCompletionAsync(request, cancellationToken))
        {
            if (item.IsToolCall)
            {
                var call = item.ToolCall;
                toolCalls.Add(new ToolCall(call.Id, call.Name, call.Arguments));
                yield return CompletionStreamEvent.ForToolCall(new ToolCallDto { Id = call.Id, Name = call.Name, Arguments = call.Arguments });
                continue;
            }

            text.Append(item.Token);
            yield return CompletionStreamEvent.ForToken(item.Token);
        }

        completion.AddMessage(toolCalls.Count > 0
            ? Message.ForToolCalls(text.ToString(), toolCalls)
            : new Message(MessageRole.Assistant, text.ToString()));
    }
}
