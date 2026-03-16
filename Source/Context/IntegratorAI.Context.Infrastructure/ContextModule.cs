using IntegratorAI.Context.Application.Commands;
using IntegratorAI.Context.Application.Queries;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Context.Contracts.Models;
using MediatR;

namespace IntegratorAI.Context.Infrastructure;

public class ContextModule : IContextModule
{
    private readonly IMediator _mediator;

    public ContextModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<Guid> CreateContext(string name, string systemRole, string domainContext, string decisionPolicy, string operatingRules, string outputFormat, IEnumerable<ToolDto> tools = null, IEnumerable<ExampleDto> examples = null, CancellationToken cancellationToken = default)
        => _mediator.Send(new CreateContextCommand(name, systemRole, domainContext, decisionPolicy, operatingRules, outputFormat, tools, examples), cancellationToken);

    public Task<string> GetContextPrompt(Guid contextId, CancellationToken cancellationToken = default)
        => _mediator.Send(new GetContextPromptQuery(contextId), cancellationToken);
}
