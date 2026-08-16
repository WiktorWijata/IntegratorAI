using IntegratorAI.Context.Application.PromptBuilder;
using IntegratorAI.Context.Application.Queries;
using IntegratorAI.Context.Domain.Repositories;
using RescuePC.Software.Domain.Exceptions;
using MediatR;

namespace IntegratorAI.Context.Application.QueryHandlers;

public class GetContextPromptQueryHandler : IRequestHandler<GetContextPromptQuery, string>
{
    private readonly IContextRepository _contextRepository;

    public GetContextPromptQueryHandler(IContextRepository contextRepository)
    {
        _contextRepository = contextRepository;
    }

    public async Task<string> Handle(GetContextPromptQuery request, CancellationToken cancellationToken)
    {
        var context = await _contextRepository.GetByIdAsync(request.ContextId, cancellationToken)
            ?? throw new NotFoundException(nameof(Context), request.ContextId);

        return ContextPromptBuilder.ToYaml(context);
    }
}
