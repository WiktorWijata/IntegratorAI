using IntegratorAI.Providers.Contracts.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IntegratorAI.Providers.Contracts
{
    public interface IProvider
    {
        Task<ProviderMessageDto> CompletionAsync(ProviderCompletionDto completion);
        Task<ProviderMessageDto> SummaryCompletionAsync(ProviderCompletionDto completion);
        IAsyncEnumerable<string> StreamCompletionAsync(ProviderCompletionDto completion, CancellationToken cancellationToken = default);
    }
}
