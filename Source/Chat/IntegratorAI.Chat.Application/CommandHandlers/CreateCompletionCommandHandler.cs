using IntegratorAI.Chat.Contracts.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.CommandHandlers;

public class CreateCompletionCommandHandler : IRequestHandler<CreateCompletionCommand, CompletionResponseDto>
{
    private readonly IProviderModule _providerModule;
    private readonly IContextModule _contextModule;
    private readonly ICompletionRepository _completionRepository;

    public CreateCompletionCommandHandler(
        IProviderModule providerModule, 
        IContextModule contextModule, 
        ICompletionRepository completionRepository)
    {
        _providerModule = providerModule;
        _contextModule = contextModule;
        _completionRepository = completionRepository;
    }

    public async Task<CompletionResponseDto> Handle(CreateCompletionCommand request, CancellationToken cancellationToken)
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
        var messageDto = await _providerModule.CompletionAsync(new CompletionDto
        {
            Messages = completion.Messages.Select(m => new MessageDto
            {
                Role = m.Role.ToString(),
                Content = m.Content
            }).ToArray()
        }, cancellationToken);

        completion.AddMessage(
            message: new Message(
                role: Enum.Parse<MessageRole>(messageDto.Role, true),
                content: messageDto.Content
            )
        );

        var response = new CompletionResponseDto
        {
            CompletionId = completion.Id.ToString(),
            Messages = completion.Messages.Select(m => new CompletionMessageDto
            {
                Role = Enum.Parse<MessageRoleDto>(m.Role.ToString(), true),
                Content = m.Content
            }).ToArray()
        };

        return response;
    }
}
