using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Infrastructure.HuggingFace;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Persistence;
using Refit;

namespace IntegratorAI.Providers.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProviders(this IServiceCollection services, string connectionString, IEnumerable<ProviderConfiguration> providersConfiguration)
    {
        var huggingFaceConfig = providersConfiguration.SingleOrDefault(p => p.Type == ProviderType.HuggingFace);
        if (huggingFaceConfig != null)
        {
            var refitSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            services.AddRefitClient<IHuggingFaceApi>(refitSettings)
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(huggingFaceConfig.BaseUrl);
                    c.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("HF_API_KEY"));
                });
        }

        services.AddKeyedScoped<IProvider, HuggingFaceProvider>(ProviderType.HuggingFace);
        services.AddScoped<IProviderModule, ProviderModule>();
        services.AddEntityFramework(connectionString);

        return services;
    }
}
