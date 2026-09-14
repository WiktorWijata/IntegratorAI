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
        /// </summary>
        [Post("/streamchat/completions")]
        Task<HttpResponseMessage> CreateCompletion([Body] CompletionRequest request, [Header("Context-Id")] Guid? contextId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Continues an existing streaming completion by appending a follow-up prompt.
        /// </summary>
        [Post("/streamchat/completions/{id}")]
        Task<HttpResponseMessage> ContinueCompletion(Guid id, [Body] CompletionRequest request, CancellationToken cancellationToken = default);
    }
}
