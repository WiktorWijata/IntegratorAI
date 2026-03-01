using IntegratorAI.Chat.Domain.Repositories;
using IntegratorAI.Chat.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.Chat.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChatDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ICompletionRepository, CompletionRepository>();
    }
}
