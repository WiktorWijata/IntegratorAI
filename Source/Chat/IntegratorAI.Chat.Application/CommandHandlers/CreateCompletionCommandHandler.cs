using IntegratorAI.Chat.Contracts.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.CommandHandlers;

public class CreateCompletionCommandHandler : IRequestHandler<CreateCompletion, CompletionResponseDto>
{
    private readonly IProviderModule _providerModule;

    public CreateCompletionCommandHandler(IProviderModule providerModule)
    {
        _providerModule = providerModule;
    }

    public async Task<CompletionResponseDto> Handle(CreateCompletion request, CancellationToken cancellationToken)
    {
        var provider = await _providerModule.GetActiveProviderAsync(cancellationToken);

        var completionDto = await provider.CompletionAsync(new CompletionDto
        {
            Messages =
            [
                new MessageDto
                {
                    Role = RoleDto.User.ToString().ToLowerInvariant(),
                    Content = request.Prompt
                }
            ]
        });

        var response = new CompletionResponseDto
        {
            CompletionId = Guid.NewGuid().ToString(),
            Messages = completionDto.Messages.Select(m => new CompletionMessageDto
            {
                Role = Enum.Parse<RoleDto>(m.Role, true),
                Content = m.Content
            }).ToArray()
        };



        return response;
    }
}
