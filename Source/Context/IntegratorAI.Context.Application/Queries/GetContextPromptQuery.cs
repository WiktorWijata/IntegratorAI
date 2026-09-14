using RescuePC.Software.Caching;
using MediatR;

namespace IntegratorAI.Context.Application.Queries;

public class GetContextPromptQuery : IRequest<string>, ICacheable
{
    public GetContextPromptQuery(Guid contextId)
    {
        ContextId = contextId;
    }

    public Guid ContextId { get; }
    public string CacheKey => CacheKeys.ContextPrompt(ContextId);
    public TimeSpan? Ttl => null;
}
