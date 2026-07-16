using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;

namespace IntegratorAI.BuildingBlocks.Infrastructure.Logging;

public static class SerilogExtensions
{
    public static void AddSerilog(this IHostBuilder host)
    {
        host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
                .WriteTo.Console();

            if (context.Configuration.GetValue<bool>("Serilog:FileLogging"))
                configuration.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
        });
    }
}
