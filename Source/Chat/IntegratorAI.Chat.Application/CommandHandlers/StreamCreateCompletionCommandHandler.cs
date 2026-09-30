using System.Runtime.CompilerServices;
using System.Text;
using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
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
            systemMessage: systemMessage
        );

        await _completionRepository.AddAsync(completion, cancellationToken);

        yield return CompletionStreamEvent.ForCompletion(completion.Id);

        var completionDto = new ProviderCompletionDto
        {
            Messages = completion.ContextMessages.Select(m => new ProviderMessageDto
            {
                Role = m.Role.ToString(),
                Content = m.Content
            }).ToArray()
        };

        var fullResponse = new StringBuilder();

        await foreach (var token in _providerModule.StreamCompletionAsync(completionDto, cancellationToken))
        {
            fullResponse.Append(token);
            yield return CompletionStreamEvent.ForToken(token);
        }

        completion.AddMessage(new Message(
            role: MessageRole.Assistant,
            content: fullResponse.ToString()
        ));
    }
}
