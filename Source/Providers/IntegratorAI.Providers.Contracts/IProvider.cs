using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntegratorAI.Providers.Contracts.Models;

namespace IntegratorAI.Providers.Contracts
{
    public interface IProvider
    {
        Task<ProviderMessageDto> CompletionAsync(ProviderCompletionDto completion);
        Task<ProviderMessageDto> SummaryCompletionAsync(ProviderCompletionDto completion);
        IAsyncEnumerable<string> StreamCompletionAsync(ProviderCompletionDto completion, CancellationToken cancellationToken = default);
    }
}
