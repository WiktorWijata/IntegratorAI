using IntegratorAI.BuildingBlocks.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace IntegratorAI.BuildingBlocks.Infrastructure.Telemetry;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryInstrumentation(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter());

        return services;
    }
}
