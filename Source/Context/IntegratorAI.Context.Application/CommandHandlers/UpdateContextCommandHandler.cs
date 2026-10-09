using IntegratorAI.Context.Application.Commands;
using IntegratorAI.Context.Domain.Repositories;
using MediatR;
using RescuePC.Software.Caching;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Context.Application.CommandHandlers;

public class UpdateContextCommandHandler : IRequestHandler<UpdateContextCommand, Unit>
{
    private readonly IContextRepository _contextRepository;
    private readonly ICacheProvider _cacheProvider;

    public UpdateContextCommandHandler(IContextRepository contextRepository, ICacheProvider cacheProvider)
    {
        _contextRepository = contextRepository;
        _cacheProvider = cacheProvider;
    }

    public async Task<Unit> Handle(UpdateContextCommand request, CancellationToken cancellationToken)
    {
        var context = await _contextRepository.GetByIdAsync(request.ContextId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Context), request.ContextId);

        context.Update(
            name: request.Name,
            systemRole: request.SystemRole,
            domainContext: request.DomainContext,
            decisionPolicy: request.DecisionPolicy,
            operatingRules: request.OperatingRules,
            outputFormat: request.OutputFormat,
            tools: ContextEntityFactory.CreateTools(request.Tools),
            examples: ContextEntityFactory.CreateExamples(request.Examples));

        // The prompt is cached without expiration. The unit of work saves after this handler returns,
        // so a concurrent read between the removal and the save could re-cache the previous prompt.
        await _cacheProvider.RemoveAsync(CacheKeys.ContextPrompt(request.ContextId));

        return Unit.Value;
    }
}
