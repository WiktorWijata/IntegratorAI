using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application.Caching;
using IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;
using StackExchange.Redis;

namespace IntegratorAI.BuildingBlocks.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, RedisCacheSettings settings)
    {
        ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(settings.ConnectionString);
        services.AddSingleton(ConnectionMultiplexer.Connect(settings.ConnectionString));
        services.AddSingleton(settings);
        services.AddSingleton<ICacheProvider, RedisCacheProvider>();
        return services;
    }
}
