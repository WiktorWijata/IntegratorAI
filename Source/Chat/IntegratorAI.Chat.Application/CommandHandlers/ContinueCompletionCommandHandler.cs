using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using RescuePC.Software.Domain.Exceptions;
using MediatR;

namespace IntegratorAI.Chat.Application.CommandHandlers;

public class ContinueCompletionCommandHandler : IRequestHandler<ContinueCompletionCommand, CompletionDto>
{
    private readonly IProviderModule _providerModule;
    private readonly ICompletionRepository _completionRepository;
    private readonly ChatSettings _chatSettings;

    public ContinueCompletionCommandHandler(IProviderModule providerModule, ICompletionRepository completionRepository, ChatSettings chatSettings)
    {
        _providerModule = providerModule;
        _completionRepository = completionRepository;
        _chatSettings = chatSettings;
    }

    public async Task<CompletionDto> Handle(ContinueCompletionCommand request, CancellationToken cancellationToken)
    {
        var completion = await _completionRepository.GetByIdAsync(request.CompletionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Completion), request.CompletionId);

        completion.AddMessage(new Message(MessageRole.User, request.Prompt));

        var completionDto = new ProviderCompletionDto
        {
            Messages = completion.ContextMessages.Select(m => new ProviderMessageDto
            {
                Role = m.Role.ToString(),
                Content = m.Content
            }).ToArray()
        };

        var messageDto = await _providerModule.CompletionAsync(completionDto, cancellationToken);

        completion.AddMessage(new Message(
            role: Enum.Parse<MessageRole>(messageDto.Role, true),
            content: messageDto.Content
        ));

        completion.TrySummarize(_chatSettings.SummarizationThreshold);

        return new CompletionDto
        {
            CompletionId = completion.Id.ToString(),
            Messages = completion.Messages.Select(m => new MessageDto
            {
                Role = Enum.Parse<MessageRoleDto>(m.Role.ToString(), true),
                Content = m.Content
            }).ToArray()
        };
    }
}
