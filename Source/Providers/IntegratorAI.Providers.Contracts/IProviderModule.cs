using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntegratorAI.Providers.Contracts.Models;

namespace IntegratorAI.Providers.Contracts
{
    public interface IProviderModule
    {
        Task<ProviderMessageDto> CompletionAsync(ProviderCompletionDto completion, CancellationToken cancellationToken = default);
        Task<ProviderMessageDto> SummaryCompletionAsync(ProviderCompletionDto completion, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> StreamCompletionAsync(ProviderCompletionDto completion, CancellationToken cancellationToken = default);
    }
}
