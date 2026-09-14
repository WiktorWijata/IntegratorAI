using System;
using System.Threading;
using System.Threading.Tasks;
using Refit;

namespace IntegratorAI.Api.Contracts.Context
{
    /// <summary>
    /// Refit client interface for the Context API.
    /// </summary>
    public interface IContextApi
    {
        /// <summary>
        /// Creates a new context and returns its unique identifier.
        /// Pass the returned identifier as the <c>Context-Id</c> header when creating a completion.
        /// </summary>
        [Post("/context")]
        Task<Guid> CreateContext([Body] ContextRequest request, CancellationToken cancellationToken = default);
    }
}
