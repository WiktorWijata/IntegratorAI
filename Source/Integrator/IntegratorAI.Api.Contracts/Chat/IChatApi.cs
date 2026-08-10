using System;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace IntegratorAI.Api.Contracts.Chat
{
    /// <summary>
    /// Refit client interface for the Chat API.
    /// </summary>
    public interface IChatApi
    {
        /// <summary>
        /// Retrieves an existing completion by its unique identifier.
        /// </summary>
        [Get("/chat/completions/{id}")]
        Task<CompletionResponse> GetCompletion(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new completion from the provided prompt.
        /// Optionally applies a previously created context via the <c>Context-Id</c> header.
        /// </summary>
        [Post("/chat/completions")]
        Task<CompletionResponse> CreateCompletion([Body] CompletionRequest request, [Header("Context-Id")] Guid? contextId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Continues an existing completion by appending a follow-up prompt.
        /// </summary>
        [Post("/chat/completions/{id}")]
        Task<CompletionResponse> ContinueCompletion(Guid id, [Body] CompletionRequest request, CancellationToken cancellationToken = default);
    }
}
