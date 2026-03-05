using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.BuildingBlocks.Application.Behaviors;
using IntegratorAI.BuildingBlocks.Common.Models.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.BuildingBlocks.IntegrationTests.MediatR;

public class MediatRIntegrationTests
{
    private readonly IServiceCollection _services = new ServiceCollection();

    public MediatRIntegrationTests()
    {
        _services.AddMediatR(typeof(TestCommand).Assembly);
    }

    [Fact]
    public void AddMediatR_RegistersUnitOfWorkBehavior_ForCommandRequest()
    {
        var descriptor = _services.FirstOrDefault(d =>
            d.ServiceType == typeof(IPipelineBehavior<TestCommand, Unit>));

        Assert.NotNull(descriptor);
        Assert.Equal(typeof(UnitOfWorkBehavior<TestCommand, Unit>), descriptor.ImplementationType);
    }

    [Fact]
    public void AddMediatR_DoesNotRegisterUnitOfWorkBehavior_WhenRequestIsNotCommand()
    {
        var descriptor = _services.FirstOrDefault(d =>
            d.ServiceType == typeof(IPipelineBehavior<TestQuery, Unit>));

        Assert.Null(descriptor);
    }

    [Fact]
    public void AddMediatR_RegistersCommandHandler_ViaMediatR()
    {
        Assert.Contains(_services, d => d.ImplementationType == typeof(TestCommandHandler));
    }
}
