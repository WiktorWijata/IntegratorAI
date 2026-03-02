using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Chat.Application.CommandHandlers;
using IntegratorAI.Chat.Persistence;

namespace IntegratorAI.Chat.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChat(this IServiceCollection services, string connectionString)
    {
        services.AddMediatR(typeof(CreateCompletionCommandHandler).Assembly);
        services.AddEntityFramework(connectionString);
        return services;
    }
}
