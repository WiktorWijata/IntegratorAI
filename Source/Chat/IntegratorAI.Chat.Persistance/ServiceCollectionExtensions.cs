using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Persistence;
using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Chat.Persistence.Repositories;

namespace IntegratorAI.Chat.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, string connectionString)
    {
        services.AddEntityFramework<ChatDbContext>(connectionString, repositories: repos =>
        {
            repos.AddScoped<ICompletionRepository, CompletionRepository>();
            repos.AddScoped<IChatUnitOfWork>(sp => sp.GetRequiredService<ChatDbContext>());
        });
    }
}
