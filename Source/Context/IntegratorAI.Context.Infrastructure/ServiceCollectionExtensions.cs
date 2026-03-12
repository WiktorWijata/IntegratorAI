using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Context.Application.CommandHandlers;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Context.Persistence;
using IntegratorAI.Context.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.Context.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddContext(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IContextModule, ContextModule>();
        services.AddMediatR<ContextDbContext>(typeof(CreateContextCommandHandler).Assembly);
        services.AddEntityFramework(connectionString);
    }
}
