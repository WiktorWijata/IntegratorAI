using System.Runtime.CompilerServices;
using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Providers.Contracts;
using MediatR;

namespace IntegratorAI.Chat.Application.CommandHandlers;

public class StreamCreateCompletionCommandHandler : IStreamRequestHandler<StreamCreateCompletionCommand, CompletionStreamEvent>
{
    private readonly IProviderModule _providerModule;
    private readonly IContextModule _contextModule;
    private readonly ICompletionRepository _completionRepository;

    public StreamCreateCompletionCommandHandler(
        IProviderModule providerModule,
        IContextModule contextModule,
        ICompletionRepository completionRepository)
    {
        _providerModule = providerModule;
        _contextModule = contextModule;
        _completionRepository = completionRepository;
    }

    public async IAsyncEnumerable<CompletionStreamEvent> Handle(StreamCreateCompletionCommand request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Message? systemMessage = null;
        if (request.ContextId.HasValue)
        {
            var contextPrompt = await _contextModule.GetContextPrompt(request.ContextId.Value, cancellationToken);
            systemMessage = new Message(MessageRole.System, contextPrompt);
        }

        var completion = new Completion(
            message: new Message(MessageRole.User, request.Prompt),
            systemMessage: systemMessage,
            contextId: request.ContextId
        );

        await _completionRepository.AddAsync(completion, cancellationToken);

        yield return CompletionStreamEvent.ForCompletion(completion.Id);

        var tools = request.ContextId.HasValue
            ? await _contextModule.GetContextTools(request.ContextId.Value, cancellationToken)
            : null;

        var providerRequest = completion.ToProviderCompletion(tools);

        await foreach (var item in _providerModule.StreamAnswer(completion, providerRequest, cancellationToken))
        {
            yield return item;
        }
    }
}
