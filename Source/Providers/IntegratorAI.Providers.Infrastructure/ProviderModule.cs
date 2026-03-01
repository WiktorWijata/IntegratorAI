using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.Providers.Infrastructure;

public class ProviderModule : IProviderModule
{
    private readonly IProviderRepository _providerRepository;
    private readonly IServiceProvider _serviceProvider;

    public ProviderModule(IProviderRepository providerRepository, IServiceProvider serviceProvider)
    {
        _providerRepository = providerRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<IProvider> GetActiveProviderAsync(CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetActiveProviderAsync(cancellationToken);
        var apiProvider = _serviceProvider.GetKeyedService<IProvider>(provider.Type);
        if (apiProvider == null)
        {
            throw new InvalidOperationException("No provider found for the specified type.");
        }

        ((IProviderInitalizable)apiProvider).SetModel(provider.Model);        
        return apiProvider;
    }
}
