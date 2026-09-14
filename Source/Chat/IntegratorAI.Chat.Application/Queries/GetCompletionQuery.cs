using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.Queries;

public class GetCompletionQuery : IRequest<CompletionDto>
{
    public GetCompletionQuery(Guid completionId)
    {
        CompletionId = completionId;
    }

    public Guid CompletionId { get; }
}
