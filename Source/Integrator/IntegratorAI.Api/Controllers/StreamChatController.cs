using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Chat;
using IntegratorAI.Api.Results;
using IntegratorAI.Chat.Contracts;

namespace IntegratorAI.Api.Controllers;

/// <summary>
/// Handles streaming chat completions using Server-Sent Events (SSE).
/// Tokens are streamed back to the client incrementally as they are generated.
/// </summary>
[ApiController]
[Route("[controller]")]
public class StreamChatController : ControllerBase
{
    private readonly IChatModule _chatModule;

    public StreamChatController(IChatModule chatModule)
    {
        _chatModule = chatModule;
    }

    /// <summary>
    /// Creates a new streaming completion from the provided prompt.
    /// Optionally applies a previously created context to shape the assistant's behavior.
    /// The response is streamed as Server-Sent Events.
    /// </summary>
    /// <param name="completion">Request body containing the user prompt.</param>
    /// <param name="contextId">Optional identifier of an existing context to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("completions")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CompletionRequest completion, [FromHeader(Name = "Context-Id")] Guid? contextId, CancellationToken cancellationToken)
    {
        return new SseResult(_chatModule.StreamCreateCompletion(completion.Prompt, contextId, cancellationToken));
    }

    /// <summary>
    /// Continues an existing streaming completion by appending a follow-up prompt.
    /// The response is streamed as Server-Sent Events.
    /// </summary>
    /// <param name="id">Unique identifier of the completion to continue.</param>
    /// <param name="completion">Request body containing the follow-up prompt.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("completions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Continue(Guid id, [FromBody] CompletionRequest completion, CancellationToken cancellationToken)
    {
        return new SseResult(_chatModule.StreamContinueCompletion(id, completion.Prompt, cancellationToken));
    }
}
