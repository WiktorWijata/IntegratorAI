using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.Consts;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Responses;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Mapping;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter;

public class OpenRouterProvider : IProvider, IProviderInitalizable
{
    public string PrimaryModel { get; set; } = string.Empty;

    /// <summary>The model that writes the summaries; the primary model when it is not set.</summary>
    public string SummarizationModel { get; set; } = string.Empty;

    private readonly IOpenRouterApi _openRouterApi;

    public OpenRouterProvider(IOpenRouterApi openRouterApi)
    {
        _openRouterApi = openRouterApi;
    }

    public async Task<ProviderMessageDto> CompletionAsync(ProviderCompletionDto completion)
    {
        var request = completion.ToChatRequest(PrimaryModel);
        var response = await CallAsync(() => _openRouterApi.ChatAsync(request));
        var message = response.Choices?.FirstOrDefault()?.Message
            ?? throw new InvalidOperationException("Provider returned no choices.");
        return message.ToMessageDto();
    }

    public async IAsyncEnumerable<ProviderStreamEvent> StreamCompletionAsync(ProviderCompletionDto completion, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = completion.ToChatRequest(PrimaryModel, stream: true);

        using var response = await CallAsync(() => _openRouterApi.StreamChatAsync(request));

        // The call returns the raw response, so a rejected request (no credit, bad key, ...) would otherwise
        // read as an empty stream and the caller would get an answer with no text and no error.
        await EnsureSuccessAsync(response, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        // A tool call arrives in pieces (the name first, then the arguments bit by bit), so it is put together here and
        // handed over once the answer ends.
        var toolCalls = new SortedDictionary<int, PendingToolCall>();

        while (!cancellationToken.IsCancellationRequested && await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            // Lines that are not data are skipped: OpenRouter sends comments (": OPENROUTER PROCESSING") to keep the connection open.
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var json = line["data: ".Length..];

            if (json == "[DONE]")
                break;

            using var document = JsonDocument.Parse(json);

            // A failure after the answer has started arrives as a chunk with an error, not as a status code.
            if (document.RootElement.TryGetProperty("error", out var error))
                throw new ProviderException($"OpenRouter failed while answering: {Truncate(error.GetRawText())}");

            var delta = document.RootElement.Deserialize<ChatStreamChunk>()?.Choices?.FirstOrDefault()?.Delta;

            if (!string.IsNullOrEmpty(delta?.Content))
            {
                yield return ProviderStreamEvent.ForToken(delta.Content);
            }

            foreach (var piece in delta?.ToolCalls ?? [])
            {
                Merge(toolCalls, piece);
            }
        }

        foreach (var (index, pending) in toolCalls)
        {
            if (string.IsNullOrEmpty(pending.Name))
                continue;

            yield return ProviderStreamEvent.ForToolCall(new ProviderToolCallDto
            {
                Id = pending.Id ?? $"call_{index}",
                Name = pending.Name,
                Arguments = pending.Arguments.Length == 0 ? "{}" : pending.Arguments.ToString(),
            });
        }
    }

    public async Task<ProviderMessageDto> SummaryCompletionAsync(ProviderCompletionDto completion)
    {
        var summaryCompletion = new ProviderCompletionDto
        {
            Messages = new[]
            {
                new ProviderMessageDto
                {
                    Role = PromptRoles.System,
                    Content = SystemPrompts.SummarizationPrompt
                }
            }.Concat(completion.Messages.Select(AsPlainMessage)).ToArray()
        };

        var model = string.IsNullOrEmpty(SummarizationModel) ? PrimaryModel : SummarizationModel;
        var request = summaryCompletion.ToChatRequest(model);
        var response = await CallAsync(() => _openRouterApi.ChatAsync(request));
        var message = response.Choices?.FirstOrDefault()?.Message
            ?? throw new InvalidOperationException("Provider returned no choices.");

        return message.ToMessageDto();
    }

    /// <summary>
    /// The summary is asked for without tools, and a provider rejects tool calls and tool answers when the request
    /// declares no tools. So they are written down as plain text.
    /// </summary>
    private static ProviderMessageDto AsPlainMessage(ProviderMessageDto message)
    {
        if (message.ToolCalls is { Length: > 0 })
        {
            var calls = string.Join(", ", message.ToolCalls.Select(c => $"{c.Name}({c.Arguments})"));
            return new ProviderMessageDto { Role = "assistant", Content = $"{message.Content} [asked for: {calls}]".Trim() };
        }

        if (message.ToolCallId is not null)
        {
            return new ProviderMessageDto { Role = "user", Content = $"[tool result] {message.Content}" };
        }

        return message;
    }

    private static void Merge(SortedDictionary<int, PendingToolCall> toolCalls, ChatToolCall piece)
    {
        // The pieces of one call share an index; a provider that leaves it out starts a new call with every id it sends.
        var index = piece.Index ?? (piece.Id is not null ? toolCalls.Count : Math.Max(toolCalls.Count - 1, 0));

        if (!toolCalls.TryGetValue(index, out var pending))
        {
            toolCalls[index] = pending = new PendingToolCall();
        }

        pending.Id ??= piece.Id;

        if (!string.IsNullOrEmpty(piece.Function?.Name))
        {
            pending.Name = piece.Function.Name;
        }

        pending.Arguments.Append(piece.Function?.Arguments);
    }

    /// <summary>Turns a failed OpenRouter call (Refit throws for the typed ones) into a <see cref="ProviderException"/>.</summary>
    private static async Task<T> CallAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (Refit.ApiException exception)
        {
            throw new ProviderException(
                $"OpenRouter answered {(int)exception.StatusCode}: {Truncate(exception.Content)}", (int)exception.StatusCode, exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ProviderException($"OpenRouter could not be reached: {exception.Message}", innerException: exception);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new ProviderException($"OpenRouter answered {(int)response.StatusCode}: {Truncate(body)}", (int)response.StatusCode);
    }

    private static string Truncate(string? text, int maxLength = 500)
        => string.IsNullOrEmpty(text) ? "(no content)" : text.Length <= maxLength ? text : text[..maxLength] + "...";

    private sealed class PendingToolCall
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public StringBuilder Arguments { get; } = new();
    }
}
