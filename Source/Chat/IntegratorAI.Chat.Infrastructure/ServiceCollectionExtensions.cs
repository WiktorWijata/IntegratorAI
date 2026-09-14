using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Application.CommandHandlers;
using IntegratorAI.Chat.Application.EventHandlers;
using IntegratorAI.Chat.Contracts;
using IntegratorAI.Chat.Persistence;
using RescuePC.Software.Domain;

namespace IntegratorAI.Chat.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddChat(this IServiceCollection services, string connectionString, ChatSettings chatSettings)
    {
        services.AddSingleton(chatSettings);
        services.AddEntityFrameworkCoreMediatR<ChatDbContext>(typeof(CreateCompletionCommandHandler).Assembly);
        services.AddEntityFramework(connectionString);
        services.AddEventBus(typeof(CompletionSummaryRequiredEventHandler).Assembly);
        services.AddScoped<IChatModule, ChatModule>();
    }
}
