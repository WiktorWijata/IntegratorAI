using IntegratorAI.Providers.Contracts.Models;
using System.Threading.Tasks;

namespace IntegratorAI.Providers.Contracts
{
    public interface IProvider
    {
        Task<CompletionDto> CompletionAsync(CompletionDto completion);        
    }
}
