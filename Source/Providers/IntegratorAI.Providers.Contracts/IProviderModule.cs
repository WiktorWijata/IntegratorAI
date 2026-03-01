using System.Threading;
using System.Threading.Tasks;

namespace IntegratorAI.Providers.Contracts
{
    public interface IProviderModule
    {
        Task<IProvider> GetActiveProviderAsync(CancellationToken cancellationToken = default);
    }
}
