using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.Providers.Domain.Repositories;
using IntegratorAI.Providers.Persistence.Reposiitories;
using RescuePC.Software.EntityFrameworkCore;

namespace IntegratorAI.Providers.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, string connectionString)
    {
        services.AddEntityFramework<ProvidersDbContext>(connectionString, repositories: repos =>
        {
            repos.AddScoped<IProviderRepository, ProviderRepository>();
        });
    }
}
