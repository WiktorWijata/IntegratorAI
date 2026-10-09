using IntegratorAI.Context.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegratorAI.Context.IntegrationTests;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddContext_DoesNotThrow_ForAllCommandHandlers()
    {
        // The shared AddMediatR extension inspects every *CommandHandler at startup, so a handler
        // shape it does not support (e.g. IRequestHandler<TRequest> without a response) crashes the
        // API on boot while all handler unit tests still pass.
        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddContext("Server=localhost;Database=IntegratorAI;"));

        Assert.Null(exception);
    }
}
