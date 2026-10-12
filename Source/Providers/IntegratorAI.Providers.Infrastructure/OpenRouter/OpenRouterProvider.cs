using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.Consts;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter;

public class OpenRouterProvider : IProvider, IProviderInitalizable
{
    public string PrimaryModel { get; set; }

    /// <summary>The model that writes the summaries; the primary model when it is not set.</summary>
    public string SummarizationModel { get; set; }

    private readonly IOpenRouterApi _openRouterApi;

    public OpenRouterProvider(IOpenRouterApi openRouterApi)
    {
        _openRouterApi = openRouterApi;
    }

    public async Task<ProviderMessageDto> CompletionAsync(ProviderCompletionDto completion)
    {
        var request = completion.ToRequest(PrimaryModel);
        var response = await CallAsync(() => _openRouterApi.ChatAsync(request));
        var choice = response.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException("Provider returned no choices.");
        return choice.ToMessageDto();
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(ProviderCompletionDto completion, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = completion.ToRequest(PrimaryModel, stream: true);

        using var response = await CallAsync(() => _openRouterApi.StreamChatAsync(request));

        // The call returns the raw response, so a rejected request (no credit, bad key, ...) would otherwise
        // read as an empty stream and the caller would get an answer with no text and no error.
        await EnsureSuccessAsync(response, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

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

            var chunk = document.RootElement.Deserialize<MessageStreamChunk>();
            var token = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;

            if (token is not null)
            {
                yield return token;
            }
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
            }.Concat(completion.Messages).ToArray()
        };

        var model = string.IsNullOrEmpty(SummarizationModel) ? PrimaryModel : SummarizationModel;
        var request = summaryCompletion.ToRequest(model);
        var response = await CallAsync(() => _openRouterApi.ChatAsync(request));
        var choice = response.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException("Provider returned no choices.");

        return choice.ToMessageDto();
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
}
