using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Chat;
using IntegratorAI.Api.Mapping;
using IntegratorAI.Chat.Contracts;

namespace IntegratorAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatModule _chatModule;

    public ChatController(IChatModule chatModule)
    {
        _chatModule = chatModule;
    }

    [HttpGet("completions/{id:guid}")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _chatModule.GetCompletion(id, cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("completions")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CompletionRequest completion, [FromHeader(Name = "Context-Id")] Guid? contextId = null, CancellationToken cancellationToken = default)
    {
        var result = await _chatModule.CreateCompletion(completion.Prompt, contextId, cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("completions/{id:guid}")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Continue(Guid id, [FromBody] CompletionRequest completion, CancellationToken cancellationToken)
    {
        var result = await _chatModule.ContinueCompletion(id, completion.Prompt, cancellationToken);
        return Ok(result.ToResponse());
    }
}
