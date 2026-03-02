using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application;

namespace IntegratorAI.BuildingBlocks.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFramework<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<IServiceCollection>? repositories = null)
        where TContext : EfContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TContext>());

        repositories?.Invoke(services);

        return services;
    }
}
