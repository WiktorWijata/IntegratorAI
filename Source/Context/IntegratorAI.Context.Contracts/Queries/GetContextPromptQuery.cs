using System;
using MediatR;

namespace IntegratorAI.Context.Contracts.Queries
{
    public class GetContextPromptQuery : IRequest<string>
    {
        public GetContextPromptQuery(Guid contextId)
        {
            ContextId = contextId;
        }

        public Guid ContextId { get; set; }
    }
}
