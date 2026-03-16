using System.Runtime.CompilerServices;
using System.Text;
using IntegratorAI.Chat.Contracts.Commands;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.CommandHandlers;

public class StreamContinueCompletionCommandHandler : IStreamRequestHandler<StreamContinueCompletionCommand, string>
{
    private readonly IProviderModule _providerModule;
    private readonly ICompletionRepository _completionRepository;
    private readonly ChatSettings _chatSettings;

    public StreamContinueCompletionCommandHandler(IProviderModule providerModule, ICompletionRepository completionRepository, ChatSettings chatSettings)
    {
        _providerModule = providerModule;
        _completionRepository = completionRepository;
        _chatSettings = chatSettings;
    }

    public async IAsyncEnumerable<string> Handle(StreamContinueCompletionCommand request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var completion = await _completionRepository.GetByIdAsync(request.CompletionId, cancellationToken)
            ?? throw new InvalidOperationException($"Completion with id '{request.CompletionId}' was not found.");

        completion.AddMessage(new Message(MessageRole.User, request.Prompt));

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
            yield return token;
        }

        completion.AddMessage(new Message(
            role: MessageRole.Assistant,
            content: fullResponse.ToString()
        ));

        completion.TrySummarize(_chatSettings.SummarizationThreshold);
    }
}
