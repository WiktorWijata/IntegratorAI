using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Context;
using IntegratorAI.Api.Mapping;
using IntegratorAI.Context.Contracts.Commands;
using MediatR;

namespace IntegratorAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ContextController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContextController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] ContextRequest contextRequest, CancellationToken cancellationToken = default)
    {
        var command = new CreateContextCommand(
            name: contextRequest.Name,
            systemRole: contextRequest.SystemRole,
            domainContext: contextRequest.DomainContext,
            decisionPolicy: contextRequest.DecisionPolicy,
            operatingRules: contextRequest.OperatingRules,
            outputFormat: contextRequest.OutputFormat,
            tools: contextRequest.Tools.Select(t => t.ToDto()),
            examples: contextRequest.Examples.Select(e => e.ToDto())
        );

        var contextId = await _mediator.Send(command, cancellationToken);
        return Ok(contextId);
    }
}
