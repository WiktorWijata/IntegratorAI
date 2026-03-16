using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Chat;
using IntegratorAI.Api.Results;
using IntegratorAI.Chat.Contracts;

namespace IntegratorAI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamChatController : ControllerBase
{
    private readonly IChatModule _chatModule;

    public StreamChatController(IChatModule chatModule)
    {
        _chatModule = chatModule;
    }

    [HttpPost("completions")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CompletionRequest completion, [FromHeader(Name = "Context-Id")] Guid? contextId, CancellationToken cancellationToken)
    {
        return new SseResult(_chatModule.StreamCreateCompletion(completion.Prompt, contextId, cancellationToken));
    }

    [HttpPost("completions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Continue(Guid id, [FromBody] CompletionRequest completion, CancellationToken cancellationToken)
    {
        return new SseResult(_chatModule.StreamContinueCompletion(id, completion.Prompt, cancellationToken));
    }
}
