using IntegratorAI.Context.Contracts.Commands;
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
            tools: request.Tools?.Select(t => new Tool(
                name: t.Name,
                description: t.Description,
                parameters: t.Parameters?.Select(p => new ToolParameter
                {
                    Name = p.Name,
                    Type = p.Type,
                    Description = p.Description
                }).ToList(),
                guardrails: t.Guardrails?.Select(g => new Guardrail(
                    description: g.Description,
                    requiresConfirmation: g.RequiresConfirmation)).ToList()
            )).ToList(),
            examples: request.Examples?.Select(e => new Example(
                input: e.Input,
                expectedResponse: e.ExpectedResponse
            )).ToList());

        await _contextRepository.AddAsync(context, cancellationToken);
        return context.Id;
    }
}
