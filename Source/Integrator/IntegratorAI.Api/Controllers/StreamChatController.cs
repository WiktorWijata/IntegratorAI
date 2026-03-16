using IntegratorAI.Api.Contracts.Chat;
using IntegratorAI.Api.Results;
using IntegratorAI.Chat.Contracts.Commands;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace IntegratorAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public StreamChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("completions")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CompletionRequest completion, [FromHeader(Name = "Context-Id")] Guid? contextId, CancellationToken cancellationToken)
    {
        return new SseResult(_mediator.CreateStream(new StreamCreateCompletionCommand(completion.Prompt, contextId), cancellationToken));
    }

    [HttpPost("completions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Continue(Guid id, [FromBody] CompletionRequest completion, CancellationToken cancellationToken)
    {
        return new SseResult(_mediator.CreateStream(new StreamContinueCompletionCommand(id, completion.Prompt), cancellationToken));
    }
}
