using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntegratorAI.Chat.Contracts.Models;

namespace IntegratorAI.Chat.Contracts
{
    public interface IChatModule
    {
        Task<CompletionDto> CreateCompletion(string prompt, Guid? contextId, CancellationToken cancellationToken = default);
        Task<CompletionDto> ContinueCompletion(Guid completionId, string prompt, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamCreateCompletion(string prompt, Guid? contextId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamContinueCompletion(Guid completionId, string prompt, CancellationToken cancellationToken = default);
        Task<CompletionDto> GetCompletion(Guid completionId, CancellationToken cancellationToken = default);
    }
}
