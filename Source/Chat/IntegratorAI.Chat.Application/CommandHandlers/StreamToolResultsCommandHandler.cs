using System.Runtime.CompilerServices;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Providers.Contracts;
using MediatR;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Chat.Application.CommandHandlers;

/// <summary>
/// Continues a completion that stopped to wait for tools: the results are added to the conversation and the model
/// writes its answer from them (or asks for more tools).
/// </summary>
public class StreamToolResultsCommandHandler : IStreamRequestHandler<StreamToolResultsCommand, CompletionStreamEvent>
{
    private readonly IProviderModule _providerModule;
    private readonly IContextModule _contextModule;
    private readonly ICompletionRepository _completionRepository;
    private readonly ChatSettings _chatSettings;

    public StreamToolResultsCommandHandler(
        IProviderModule providerModule,
        IContextModule contextModule,
        ICompletionRepository completionRepository,
        ChatSettings chatSettings)
    {
        _providerModule = providerModule;
        _contextModule = contextModule;
        _completionRepository = completionRepository;
        _chatSettings = chatSettings;
    }

    public async IAsyncEnumerable<CompletionStreamEvent> Handle(StreamToolResultsCommand request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var completion = await _completionRepository.GetByIdAsync(request.CompletionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Completion), request.CompletionId);

        // The calls come from the conversation kept here, so the client cannot answer a call the model did not make.
        var contentByCallId = new Dictionary<string, string>();
        foreach (var result in request.Results)
        {
            if (!contentByCallId.TryAdd(result.ToolCallId, result.IsError ? $"Error: {result.Content}" : result.Content))
            {
                throw new ConflictException($"The tool call '{result.ToolCallId}' is answered more than once.");
            }
        }

        try
        {
            completion.AddToolResults(contentByCallId);
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        var tools = completion.ContextId.HasValue
            ? await _contextModule.GetContextTools(completion.ContextId.Value, cancellationToken)
            : null;

        var providerRequest = completion.ToProviderCompletion(tools);

        await foreach (var item in _providerModule.StreamAnswer(completion, providerRequest, cancellationToken))
        {
            yield return item;
        }

        completion.TrySummarize(_chatSettings.SummarizationThreshold);
    }
}
