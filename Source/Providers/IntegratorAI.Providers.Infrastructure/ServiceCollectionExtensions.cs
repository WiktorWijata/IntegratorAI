using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Infrastructure.HuggingFace;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Infrastructure.OpenRouter;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api;
using IntegratorAI.Providers.Persistence;
using RescuePC.Software.Refit;

namespace IntegratorAI.Providers.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProviders(this IServiceCollection services, string connectionString, IEnumerable<ProviderConfiguration> providersConfiguration)
    {
        var huggingFaceConfig = providersConfiguration.SingleOrDefault(p => p.Type == ProviderType.HuggingFace);
        if (huggingFaceConfig != null)
        {
            services.AddRefitClient<IHuggingFaceApi>(
                clientName: ProviderType.HuggingFace.ToString(),
                client: c =>
                {
                    c.BaseAddress = new Uri(huggingFaceConfig.BaseUrl);
                    c.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("HF_API_KEY"));
                });
        }

        var openRouterConfig = providersConfiguration.SingleOrDefault(p => p.Type == ProviderType.OpenRouter);
        if (openRouterConfig != null)
        {
            services.AddRefitClient<IOpenRouterApi>(
                clientName: ProviderType.OpenRouter.ToString(),
                client: c =>
                {
                    c.BaseAddress = new Uri(openRouterConfig.BaseUrl);
                    c.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"));
                });
        }

        services.AddKeyedScoped<IProvider, HuggingFaceProvider>(ProviderType.HuggingFace);
        services.AddKeyedScoped<IProvider, OpenRouterProvider>(ProviderType.OpenRouter);
        services.AddScoped<IProviderModule, ProviderModule>();
        services.AddEntityFramework(connectionString);

        return services;
    }
}
