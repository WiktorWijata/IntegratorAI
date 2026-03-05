using MediatR;

namespace IntegratorAI.BuildingBlocks.Common.Models.MediatR;

public class TestCommandHandler : IRequestHandler<TestCommand, Unit>
{
    public Task<Unit> Handle(TestCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(Unit.Value);
}
