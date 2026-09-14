using IntegratorAI.Context.Domain.Repositories;
using IntegratorAI.Context.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using RescuePC.Software.EntityFrameworkCore;

namespace IntegratorAI.Context.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, string connectionString)
    {
        services.AddEntityFramework<ContextDbContext>(connectionString, repositories: repos =>
        {
            repos.AddScoped<IContextRepository, ContextRepository>();
        });
    }
}
    
