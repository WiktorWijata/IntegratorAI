using IntegratorAI.BuildingBlocks.Domain;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Contracts.Queries;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using MediatR;

namespace IntegratorAI.Chat.Application.QueryHandlers;

public class GetCompletionQueryHandler : IRequestHandler<GetCompletionQuery, CompletionDto>
{
    private readonly ICompletionRepository _completionRepository;

    public GetCompletionQueryHandler(ICompletionRepository completionRepository)
    {
        _completionRepository = completionRepository;
    }

    public async Task<CompletionDto> Handle(GetCompletionQuery request, CancellationToken cancellationToken)
    {
        var completion = await _completionRepository.GetByIdAsync(request.CompletionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Completion), request.CompletionId);

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
