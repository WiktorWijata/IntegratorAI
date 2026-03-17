using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.BuildingBlocks.Application.Behaviors;
using MediatR;

namespace IntegratorAI.BuildingBlocks.Application;

public static class ServiceCollectionExtensions
{
    public static void AddMediatR<TUnitOfWork>(this IServiceCollection services, params Assembly[] assemblies)
        where TUnitOfWork : class, IUnitOfWork
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        foreach (var assembly in assemblies)
        {
            var commandHandlerTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("CommandHandler", StringComparison.Ordinal) && !t.IsAbstract);

            foreach (var handlerType in commandHandlerTypes)
            {
                foreach (var iface in handlerType.GetInterfaces().Where(i => i.IsGenericType))
                {
                    var genericDef = iface.GetGenericTypeDefinition();
                    var args = iface.GetGenericArguments();
                    var requestType = args[0];
                    var responseType = args[1];

                    if (!requestType.Name.EndsWith("Command", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (genericDef == typeof(IRequestHandler<,>))
                    {
                        services.AddTransient(
                            typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType),
                            typeof(UnitOfWorkBehavior<,,>).MakeGenericType(requestType, responseType, typeof(TUnitOfWork))
                        );
                    }
                    else if (genericDef == typeof(IStreamRequestHandler<,>))
                    {
                        services.AddTransient(
                            typeof(IStreamPipelineBehavior<,>).MakeGenericType(requestType, responseType),
                            typeof(StreamUnitOfWorkBehavior<,,>).MakeGenericType(requestType, responseType, typeof(TUnitOfWork))
                        );
                    }
                }
            }
        }
    }
}
