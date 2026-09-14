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
        var response = await _huggingFaceApi.ChatAsync(request);
        var choice = response.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException("Provider returned no choices.");
        return choice.ToMessageDto();
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(ProviderCompletionDto completion, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = completion.ToRequest(PrimaryModel, stream: true);

        using var response = await _huggingFaceApi.StreamChatAsync(request);

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
            var response = await _huggingFaceApi.PipelineAsync<SummarizationResponse[]>(
                modelId: SummarizationModel,
                request: new PipelineRequest
                {
                    Inputs = input,
                    Parameters = new PipelineParameters { Truncation = true }
                }
            );

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
        var chatResponse = await _huggingFaceApi.ChatAsync(chatRequest);
        var choice = chatResponse.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException("Provider returned no choices.");

        return choice.ToMessageDto();
    }
}
