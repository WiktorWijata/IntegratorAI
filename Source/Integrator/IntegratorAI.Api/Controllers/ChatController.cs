using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Chat;
using IntegratorAI.Api.Mapping;
using IntegratorAI.Chat.Contracts.Commands;
using MediatR;

namespace IntegratorAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("completions")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CompletionRequest completion, [FromQuery] bool useContext = false, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new CreateCompletionCommand(completion.Prompt), cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("completions/{id:guid}")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Continue(Guid id, [FromBody] CompletionRequest completion, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ContinueCompletionCommand(id, completion.Prompt), cancellationToken);
        return Ok(result.ToResponse());
    }
}
