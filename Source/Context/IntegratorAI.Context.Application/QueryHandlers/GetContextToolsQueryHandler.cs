using IntegratorAI.Context.Application.Queries;
using IntegratorAI.Context.Contracts.Models;
using IntegratorAI.Context.Domain.Repositories;
using MediatR;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Context.Application.QueryHandlers;

public class GetContextToolsQueryHandler : IRequestHandler<GetContextToolsQuery, IReadOnlyList<ToolDto>>
{
    private readonly IContextRepository _contextRepository;

    public GetContextToolsQueryHandler(IContextRepository contextRepository)
    {
        _contextRepository = contextRepository;
    }

    public async Task<IReadOnlyList<ToolDto>> Handle(GetContextToolsQuery request, CancellationToken cancellationToken)
    {
        var context = await _contextRepository.GetByIdAsync(request.ContextId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Context), request.ContextId);

        return context.Tools
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .Select(t => new ToolDto(
                t.Name,
                t.Description,
                t.Parameters.Select(p => new ToolParameterDto(p.Name, p.Type, p.Description, p.Required)).ToList()))
            .ToList();
    }
}
