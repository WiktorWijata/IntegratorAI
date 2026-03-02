using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IntegratorAI.BuildingBlocks.Application.Caching;
using IntegratorAI.BuildingBlocks.Domain.Event;
using IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;
using IntegratorAI.BuildingBlocks.Infrastructure.Event;
using StackExchange.Redis;

namespace IntegratorAI.BuildingBlocks.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.TryAddScoped<IEventBus, EventBus>();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleEvent<>))
                    .Select(i => new { Implementation = t, ServiceType = i }));

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.ServiceType, handler.Implementation);
            }
        }

        return services;
    }

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
