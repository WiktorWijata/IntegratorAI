using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.Consts;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;

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

    public async Task<MessageDto> CompletionAsync(CompletionDto completion)
    {
        var request = completion.ToRequest(PrimaryModel);
        var response = await _huggingFaceApi.ChatAsync(request);
        var choice = response.Choices?.FirstOrDefault()
            ?? throw new InvalidOperationException("Provider returned no choices.");
        return choice.ToMessageDto();
    }

    public async Task<MessageDto> SummaryCompletionAsync(CompletionDto completion)
    {
        var input = string.Join("\n", completion.Messages.Select(m => $"{m.Role}: {m.Content}"));

        if (!string.IsNullOrEmpty(SummarizationModel))
        {
            var response = await _huggingFaceApi.PipelineAsync<SummarizationResponse[]>(
                modelId: SummarizationModel,
                request: new PipelineRequest { Inputs = input }
            );

            var summary = response.SingleOrDefault()?.SummaryText
                ?? throw new InvalidOperationException("Summarization pipeline returned no result.");

            return new MessageDto
            {
                Role = PromptRoles.System,
                Content = summary
            };
        }

        var summaryCompletion = new CompletionDto
        {
            Messages = new[]
            {
                new MessageDto
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
