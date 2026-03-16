using System;
using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Contracts.Queries
{
    public class GetCompletionQuery : IRequest<CompletionDto>
    {
        public GetCompletionQuery(Guid completionId)
        {
            CompletionId = completionId;
        }

        public Guid CompletionId { get; }
    }
}
