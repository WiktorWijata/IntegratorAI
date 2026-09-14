using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Providers.Persistence.Reposiitories;

public class ProviderRepository : IProviderRepository
{
    private readonly ProvidersDbContext _context;

    public ProviderRepository(ProvidersDbContext context)
    {
        _context = context;
    }

    public async Task<Provider> GetActiveProviderAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Providers.SingleAsync(x => x.IsActive, cancellationToken);
    }
}
