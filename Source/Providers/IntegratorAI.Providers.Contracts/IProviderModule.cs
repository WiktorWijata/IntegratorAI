using System.Threading;
using System.Threading.Tasks;
using IntegratorAI.Providers.Contracts.Models;

namespace IntegratorAI.Providers.Contracts
{
    public interface IProviderModule
    {
        Task<MessageDto> CompletionAsync(CompletionDto completion, CancellationToken cancellationToken = default);
        Task<MessageDto> SummaryCompletionAsync(CompletionDto completion, CancellationToken cancellationToken = default);
    }
}
