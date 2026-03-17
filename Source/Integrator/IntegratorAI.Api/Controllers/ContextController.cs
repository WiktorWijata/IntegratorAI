using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Context;
using IntegratorAI.Api.Mapping;
using IntegratorAI.Context.Contracts;

namespace IntegratorAI.Api.Controllers;

/// <summary>
/// Manages AI contexts that define the assistant's persona, domain knowledge and behavioral rules.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ContextController : ControllerBase
{
    private readonly IContextModule _contextModule;

    public ContextController(IContextModule contextModule)
    {
        _contextModule = contextModule;
    }

    /// <summary>
    /// Creates a new context and returns its unique identifier.
    /// Pass the returned identifier as the <c>Context-Id</c> header when creating a completion
    /// to apply this context to the conversation.
    /// </summary>
    /// <param name="contextRequest">Request body describing the context configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the newly created context.</returns>
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
