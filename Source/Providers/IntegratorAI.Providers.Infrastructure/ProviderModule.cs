using IntegratorAI.BuildingBlocks.Application.Caching;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.Providers.Infrastructure;

public class ProviderModule : IProviderModule
{
    private readonly IProviderRepository _providerRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheProvider _cacheProvider;

    public ProviderModule(IProviderRepository providerRepository, IServiceProvider serviceProvider, ICacheProvider cacheProvider)
    {
        _providerRepository = providerRepository;
        _serviceProvider = serviceProvider;
        _cacheProvider = cacheProvider;
    }

    public async Task<IProvider> GetActiveProviderAsync(CancellationToken cancellationToken = default)
    {
        var provider = await _cacheProvider.GetAsync<Provider>(CacheKeys.ActiveProvider);

        if (provider is null)
        {
            provider = await _providerRepository.GetActiveProviderAsync(cancellationToken);
            await _cacheProvider.SetAsync(CacheKeys.ActiveProvider, provider);
        }

        var apiProvider = _serviceProvider.GetKeyedService<IProvider>(provider.Type);
        if (apiProvider == null)
        {
            throw new InvalidOperationException("No provider found for the specified type.");
        }

        ((IProviderInitalizable)apiProvider).SetModel(provider.Model);
        return apiProvider;
    }
}
