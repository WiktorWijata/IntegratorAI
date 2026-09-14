using Microsoft.AspNetCore.Mvc;
using IntegratorAI.Api.Contracts.Chat;
using IntegratorAI.Api.Mapping;
using IntegratorAI.Chat.Contracts;

namespace IntegratorAI.Api.Controllers;

/// <summary>
/// Manages chat completions — create, retrieve and continue conversations with the AI assistant.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatModule _chatModule;

    public ChatController(IChatModule chatModule)
    {
        _chatModule = chatModule;
    }

    /// <summary>
    /// Retrieves an existing completion by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the completion.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The completion with its full conversation history.</returns>
    [HttpGet("completions/{id:guid}")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _chatModule.GetCompletion(id, cancellationToken);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Creates a new completion from the provided prompt.
    /// Optionally applies a previously created context to shape the assistant's behavior.
    /// </summary>
    /// <param name="completion">Request body containing the user prompt.</param>
    /// <param name="contextId">Optional identifier of an existing context to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created completion with the assistant's response.</returns>
    [HttpPost("completions")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CompletionRequest completion, [FromHeader(Name = "Context-Id")] Guid? contextId = null, CancellationToken cancellationToken = default)
    {
        var result = await _chatModule.CreateCompletion(completion.Prompt, contextId, cancellationToken);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Continues an existing completion by appending a follow-up prompt to the conversation.
    /// </summary>
    /// <param name="id">Unique identifier of the completion to continue.</param>
    /// <param name="completion">Request body containing the follow-up prompt.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated completion with the assistant's response appended.</returns>
    [HttpPost("completions/{id:guid}")]
    [ProducesResponseType(typeof(CompletionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Continue(Guid id, [FromBody] CompletionRequest completion, CancellationToken cancellationToken)
    {
        var result = await _chatModule.ContinueCompletion(id, completion.Prompt, cancellationToken);
        return Ok(result.ToResponse());
    }
}
