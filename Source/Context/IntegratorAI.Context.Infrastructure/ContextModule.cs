using IntegratorAI.Context.Contracts;
using IntegratorAI.Context.Contracts.Queries;
using MediatR;

namespace IntegratorAI.Context.Infrastructure;

public class ContextModule : IContextModule
{
    private readonly IMediator _mediator;

    public ContextModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> GetContextPrompt(Guid contextId, CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetContextPromptQuery(contextId), cancellationToken);
    }
}
