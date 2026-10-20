using IntegratorAI.Context.Contracts.Models;
using MediatR;

namespace IntegratorAI.Context.Application.Queries;

public class GetContextToolsQuery : IRequest<IReadOnlyList<ToolDto>>
{
    public GetContextToolsQuery(Guid contextId)
    {
        ContextId = contextId;
    }

    public Guid ContextId { get; }
}
