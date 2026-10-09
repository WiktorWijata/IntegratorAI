using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.Consts;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace;

public class HuggingFaceProvider : IProvider, IProviderInitalizable
{
    public string PrimaryModel { get; set; }
    public string SummarizationModel { get; set; }

    private readonly IHuggingFaceApi _huggingFaceApi;

    public HuggingFaceProvider(IHuggingFaceApi huggingFaceApi)
    {
        _huggingFaceApi = huggingFaceApi;
    }

    public async Task<ProviderMessageDto> CompletionAsync(ProviderCompletionDto completion)
    {
        var request = completion.ToRequest(PrimaryModel);
        var response = await CallAsync(() => _huggingFaceApi.ChatAsync(request));
        var choice = response.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException("Provider returned no choices.");
        return choice.ToMessageDto();
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(ProviderCompletionDto completion, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = completion.ToRequest(PrimaryModel, stream: true);

        using var response = await CallAsync(() => _huggingFaceApi.StreamChatAsync(request));

        // The call returns the raw response, so a rejected request (no credit, bad key, ...) would otherwise
        // read as an empty stream and the caller would get an answer with no text and no error.
        await EnsureSuccessAsync(response, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var json = line["data: ".Length..];

            if (json == "[DONE]")
            {
                break;
            }
                

            var chunk = JsonSerializer.Deserialize<MessageStreamChunk>(json);
            var token = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;

            if (token is not null)
            {
                yield return token;
            }
        }
    }

    public async Task<ProviderMessageDto> SummaryCompletionAsync(ProviderCompletionDto completion)
    {
        var input = string.Join("\n", completion.Messages
            .Where(x => x.Role.ToLowerInvariant() != PromptRoles.System.ToLowerInvariant())
            .Select(m => $"{m.Role}: {m.Content}"));

        if (!string.IsNullOrEmpty(SummarizationModel))
        {
            var response = await CallAsync(() => _huggingFaceApi.PipelineAsync<SummarizationResponse[]>(
                modelId: SummarizationModel,
                request: new PipelineRequest
                {
                    Inputs = input,
                    Parameters = new PipelineParameters { Truncation = true }
                }
            ));

            var summary = response.SingleOrDefault()?.SummaryText
                ?? throw new InvalidOperationException("Summarization pipeline returned no result.");

            return new ProviderMessageDto
            {
                Role = PromptRoles.System,
                Content = summary
            };
        }

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

        var chatRequest = summaryCompletion.ToRequest(PrimaryModel);
        var chatResponse = await CallAsync(() => _huggingFaceApi.ChatAsync(chatRequest));
        var choice = chatResponse.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException("Provider returned no choices.");

        return choice.ToMessageDto();
    }

    /// <summary>Turns a failed HuggingFace call (Refit throws for the typed ones) into a <see cref="ProviderException"/>.</summary>
    private static async Task<T> CallAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (Refit.ApiException exception)
        {
            throw new ProviderException(
                $"HuggingFace answered {(int)exception.StatusCode}: {Truncate(exception.Content)}", (int)exception.StatusCode, exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ProviderException($"HuggingFace could not be reached: {exception.Message}", innerException: exception);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new ProviderException($"HuggingFace answered {(int)response.StatusCode}: {Truncate(body)}", (int)response.StatusCode);
    }

    private static string Truncate(string? text, int maxLength = 500)
        => string.IsNullOrEmpty(text) ? "(no content)" : text.Length <= maxLength ? text : text[..maxLength] + "...";
}
