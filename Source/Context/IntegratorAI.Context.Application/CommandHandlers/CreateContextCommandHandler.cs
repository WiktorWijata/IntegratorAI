using IntegratorAI.Context.Application.Commands;
using IntegratorAI.Context.Domain;
using IntegratorAI.Context.Domain.Repositories;
using MediatR;

namespace IntegratorAI.Context.Application.CommandHandlers;

public class CreateContextCommandHandler : IRequestHandler<CreateContextCommand, Guid>
{
    private readonly IContextRepository _contextRepository;

    public CreateContextCommandHandler(IContextRepository contextRepository)
    {
        _contextRepository = contextRepository;
    }

    public async Task<Guid> Handle(CreateContextCommand request, CancellationToken cancellationToken)
    {
        var context = new Domain.Context(
            name: request.Name,
            systemRole: request.SystemRole,
            domainContext: request.DomainContext,
            decisionPolicy: request.DecisionPolicy,
            operatingRules: request.OperatingRules,
            outputFormat: request.OutputFormat,
            tools: ContextEntityFactory.CreateTools(request.Tools),
            examples: ContextEntityFactory.CreateExamples(request.Examples));

        await _contextRepository.AddAsync(context, cancellationToken);
        return context.Id;
    }
}
