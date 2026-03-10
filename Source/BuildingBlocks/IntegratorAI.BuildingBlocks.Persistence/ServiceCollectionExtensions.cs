using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application;

namespace IntegratorAI.BuildingBlocks.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework<TContext>(this IServiceCollection services,
        string connectionString,
        Action<IServiceCollection>? repositories = null)
        where TContext : EfContext
    {
        services.AddScoped<PublishEventsInterceptor>();

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<PublishEventsInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TContext>());

        repositories?.Invoke(services);
    }
}
