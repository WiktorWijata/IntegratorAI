using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntegratorAI.Context.Contracts
{
    public interface IContextModule
    {
        Task<string> GetContextPrompt(Guid contextId, CancellationToken cancellationToken = default);
    }
}
