using System;
using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Contracts.Commands
{
    public class ContinueCompletionCommand : IRequest<CompletionDto>
    {
        public ContinueCompletionCommand(Guid completionId, string prompt)
        {
            CompletionId = completionId;
            Prompt = prompt;
        }

        public Guid CompletionId { get; set; }
        public string Prompt { get; set; }
    }
}
