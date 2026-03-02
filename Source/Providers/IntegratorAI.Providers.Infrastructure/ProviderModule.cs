using IntegratorAI.BuildingBlocks.Application.Caching;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.Providers.Infrastructure;

public class ProviderModule : IProviderModule
{
    private readonly IProviderRepository _providerRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheProvider _cacheProvider;
    private IProvider? _provider;

    public ProviderModule(IProviderRepository providerRepository, IServiceProvider serviceProvider, ICacheProvider cacheProvider)
    {
        _providerRepository = providerRepository;
        _serviceProvider = serviceProvider;
        _cacheProvider = cacheProvider;
    }

    public async Task<MessageDto> CompletionAsync(CompletionDto completion, CancellationToken cancellationToken = default)
    {
        var provider = await GetActiveProviderAsync(cancellationToken);
        return await provider.CompletionAsync(completion);
    }

    public async Task<MessageDto> SummaryCompletionAsync(CompletionDto completion, CancellationToken cancellationToken = default)
    {
        var provider = await GetActiveProviderAsync(cancellationToken);
        return await provider.SummaryCompletionAsync(completion);
    }

    private async Task<IProvider> GetActiveProviderAsync(CancellationToken cancellationToken = default)
    {
        if (_provider is not null)
            return _provider;

        var provider = await _cacheProvider.GetAsync<Provider>(CacheKeys.ActiveProvider);

        if (provider is null)
        {
            provider = await _providerRepository.GetActiveProviderAsync(cancellationToken);
            await _cacheProvider.SetAsync(CacheKeys.ActiveProvider, provider);
        }

        var apiProvider = _serviceProvider.GetKeyedService<IProvider>(provider.Type)
            ?? throw new InvalidOperationException("No provider found for the specified type.");

        ((IProviderInitalizable)apiProvider).SetPrimaryModel(provider.PrimaryModel);
        ((IProviderInitalizable)apiProvider).SetSummarizationModel(provider.SummarizationModel);

        return _provider = apiProvider;
    }
}
