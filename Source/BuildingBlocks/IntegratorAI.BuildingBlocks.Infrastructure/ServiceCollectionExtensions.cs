using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application.Caching;
using IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;
using StackExchange.Redis;

namespace IntegratorAI.BuildingBlocks.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseRedisAsDefaultCacheProvider(this IServiceCollection services, RedisCacheSettings settings)
    {
        var configurationOptions = ConfigurationOptions.Parse(settings.ConnectionString);

        if (!string.IsNullOrEmpty(settings.Password))
        {
            configurationOptions.Password = settings.Password;
        }

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configurationOptions));
        services.AddSingleton(settings);
        services.AddSingleton<ICacheProvider, RedisCacheProvider>();
        return services;
    }
}
