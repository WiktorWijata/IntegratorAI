using MediatR;

namespace IntegratorAI.BuildingBlocks.Common.Models.MediatR;

public record TestCommand : IRequest<Unit>;
