using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RescuePC.Software.EntityFrameworkCore;
using RescuePC.Software.EntityFrameworkCore.MediatR;
using RescuePC.Software.Logging.Behaviors;
using RescuePC.Software.Caching.Behaviors;

namespace IntegratorAI.BuildingBlocks.Application;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFrameworkCoreMediatR<TUnitOfWork>(this IServiceCollection services, params Assembly[] assemblies)
        where TUnitOfWork : class, IUnitOfWork
    {
        services.AddMediatR<TUnitOfWork>(assemblies, [typeof(LoggingBehavior<,>), typeof(CachingBehavior<,>)]);
    }
}
