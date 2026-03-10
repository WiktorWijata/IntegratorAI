using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application.Behaviors;
using MediatR;

namespace IntegratorAI.BuildingBlocks.Application;

public static class ServiceCollectionExtensions
{
    public static void AddMediatR(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        foreach (var assembly in assemblies)
        {
            var commandTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("CommandHandler", StringComparison.Ordinal) && !t.IsAbstract)
                .SelectMany(h => h.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    .Select(i => new
                    {
                        RequestType = i.GetGenericArguments()[0],
                        ResponseType = i.GetGenericArguments()[1]
                    }));

            foreach (var command in commandTypes)
            {
                if (!command.RequestType.Name.EndsWith("Command", StringComparison.Ordinal))
                {
                    continue;
                }

                services.AddTransient(
                    typeof(IPipelineBehavior<,>).MakeGenericType(command.RequestType, command.ResponseType),
                    typeof(UnitOfWorkBehavior<,>).MakeGenericType(command.RequestType, command.ResponseType)
                );
            }
        }
    }
}
