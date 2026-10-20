using System.Runtime.CompilerServices;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Providers.Contracts;
using RescuePC.Software.Domain.Exceptions;
using MediatR;

namespace IntegratorAI.Chat.Application.CommandHandlers;

public class StreamContinueCompletionCommandHandler : IStreamRequestHandler<StreamContinueCompletionCommand, CompletionStreamEvent>
{
    private readonly IProviderModule _providerModule;
    private readonly IContextModule _contextModule;
    private readonly ICompletionRepository _completionRepository;
    private readonly ChatSettings _chatSettings;

    public StreamContinueCompletionCommandHandler(IProviderModule providerModule, IContextModule contextModule, ICompletionRepository completionRepository, ChatSettings chatSettings)
    {
        _providerModule = providerModule;
        _contextModule = contextModule;
        _completionRepository = completionRepository;
        _chatSettings = chatSettings;
    }

    public async IAsyncEnumerable<CompletionStreamEvent> Handle(StreamContinueCompletionCommand request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var completion = await _completionRepository.GetByIdAsync(request.CompletionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Completion), request.CompletionId);

        // The model is waiting for tools, so the conversation can only go on with their results.
        if (completion.PendingToolCalls.Count > 0)
        {
            throw new ConflictException("The completion is waiting for tool results.");
        }

        completion.AddMessage(new Message(MessageRole.User, request.Prompt));

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
