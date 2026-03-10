using IntegratorAI.Chat.Contracts.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.CommandHandlers;

public class ContinueCompletionCommandHandler : IRequestHandler<ContinueCompletionCommand, CompletionResponseDto>
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

    public async Task<CompletionResponseDto> Handle(ContinueCompletionCommand request, CancellationToken cancellationToken)
    {
        var completion = await _completionRepository.GetByIdAsync(request.CompletionId, cancellationToken);
        if (completion == null) 
        {
            throw new InvalidOperationException($"Completion with id '{request.CompletionId}' was not found.");
        }

        completion.AddMessage(new Message(MessageRole.User, request.Prompt));

        var messages = new List<MessageDto>();

        if (completion.Summary is not null)
        {
            messages.Add(new MessageDto
            {
                Role = nameof(MessageRole.System),
                Content = $"Summary of the conversation so far: {completion.Summary.Content}"
            });
        }

        messages.AddRange(completion.UnsummarizedMessages.Select(m => new MessageDto
        {
            Role = m.Role.ToString(),
            Content = m.Content
        }));

        var completionDto = new CompletionDto { Messages = [.. messages] };

        var messageDto = await _providerModule.CompletionAsync(completionDto, cancellationToken);

        completion.AddMessage(new Message(
            role: Enum.Parse<MessageRole>(messageDto.Role, true),
            content: messageDto.Content
        ));

        completion.TrySummarize(_chatSettings.SummarizationThreshold);

        return new CompletionResponseDto
        {
            CompletionId = completion.Id.ToString(),
            Messages = completion.Messages.Select(m => new CompletionMessageDto
            {
                Role = Enum.Parse<MessageRoleDto>(m.Role.ToString(), true),
                Content = m.Content
            }).ToArray()
        };
    }
}
