using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace IntegratorAI.Api.Contracts.Chat
{
    /// <summary>
    /// Refit client interface for the streaming Chat API (Server-Sent Events).
    /// Use <see cref="HttpResponseMessage"/> to read the SSE stream manually.
    /// </summary>
    public interface IStreamChatApi
    {
        /// <summary>
        /// Creates a new streaming completion from the provided prompt.
        /// Optionally applies a previously created context via the <c>Context-Id</c> header.
        /// The identifier of the new completion is returned in the <c>Completion-Id</c> response header.
        /// </summary>
        [Post("/streamchat/completions")]
        Task<HttpResponseMessage> CreateCompletion([Body] CompletionRequest request, [Header("Context-Id")] Guid? contextId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Continues an existing streaming completion by appending a follow-up prompt.
        /// </summary>
        [Post("/streamchat/completions/{id}")]
        Task<HttpResponseMessage> ContinueCompletion(Guid id, [Body] CompletionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Answers the tools the model asked for. A stream may end with <c>tool_call</c> events (<c>event: tool_call</c>,
        /// data <c>{"id": "...", "name": "...", "arguments": {...}}</c>) instead of an answer; the completion then waits
        /// until every call is answered here, and the response is a stream again: the answer, or more tool calls.
        /// Answers 409 when the completion is not waiting for tools or the results do not match the calls.
        /// </summary>
        [Post("/streamchat/completions/{id}/tool-results")]
        Task<HttpResponseMessage> SubmitToolResults(Guid id, [Body] ToolResultsRequest request, CancellationToken cancellationToken = default);
    }
}
