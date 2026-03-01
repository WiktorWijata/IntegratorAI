using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.Providers.Domain.Repositories;
using IntegratorAI.Providers.Persistence.Reposiitories;

namespace IntegratorAI.Providers.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ProvidersDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IProviderRepository, ProviderRepository>();
    }
}
