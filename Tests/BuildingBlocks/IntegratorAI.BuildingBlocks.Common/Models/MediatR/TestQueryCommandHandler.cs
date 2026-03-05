using MediatR;

namespace IntegratorAI.BuildingBlocks.Common.Models.MediatR;

public class TestQueryCommandHandler : IRequestHandler<TestQuery, Unit>
{
    public Task<Unit> Handle(TestQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Unit.Value);
}
