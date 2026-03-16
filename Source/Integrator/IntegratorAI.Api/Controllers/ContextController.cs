using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Context;
using IntegratorAI.Api.Mapping;
using IntegratorAI.Context.Contracts;

namespace IntegratorAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ContextController : ControllerBase
{
    private readonly IContextModule _contextModule;

    public ContextController(IContextModule contextModule)
    {
        _contextModule = contextModule;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] ContextRequest contextRequest, CancellationToken cancellationToken = default)
    {
        var contextId = await _contextModule.CreateContext(
            name: contextRequest.Name,
            systemRole: contextRequest.SystemRole,
            domainContext: contextRequest.DomainContext,
            decisionPolicy: contextRequest.DecisionPolicy,
            operatingRules: contextRequest.OperatingRules,
            outputFormat: contextRequest.OutputFormat,
            tools: contextRequest.Tools.Select(t => t.ToDto()),
            examples: contextRequest.Examples.Select(e => e.ToDto()),
            cancellationToken: cancellationToken
        );
        return Ok(contextId);
    }
}
