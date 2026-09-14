namespace IntegratorAI.Providers.Domain.Repositories;

public interface IProviderRepository
{
    Task<Provider> GetActiveProviderAsync(CancellationToken cancellationToken = default);
}
