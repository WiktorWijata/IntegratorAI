using IntegratorAI.Context.Contracts.Queries;
using IntegratorAI.Context.Domain.Repositories;
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
        var context = await _contextRepository.GetByIdAsync(request.ContextId, cancellationToken);
        if (context is null)
        {
            throw new InvalidOperationException($"Context with id '{request.ContextId}' was not found.");
        }

        return context.SystemPrompt;
    }
}
