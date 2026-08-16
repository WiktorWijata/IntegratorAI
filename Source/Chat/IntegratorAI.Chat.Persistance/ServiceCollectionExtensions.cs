using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Chat.Persistence.Repositories;
using RescuePC.Software.EntityFrameworkCore;
using RescuePC.Software.EntityFrameworkCore.Domain.Interceptors;
using RescuePC.Software.EntityFrameworkCore.Domain;

namespace IntegratorAI.Chat.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, string connectionString)
    {
        services.AddPublishEventsInterceptor();

        services.AddEntityFramework<ChatDbContext>(connectionString,
            interceptors: [typeof(PublishEventsInterceptor)],
            repositories: repos =>
            {
                repos.AddScoped<ICompletionRepository, CompletionRepository>();
                repos.AddScoped<IChatUnitOfWork>(sp => sp.GetRequiredService<ChatDbContext>());
            });
    }
}
