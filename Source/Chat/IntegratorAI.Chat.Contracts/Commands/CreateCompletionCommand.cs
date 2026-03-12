using System;
using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Contracts.Commands
{
    public class CreateCompletionCommand : IRequest<CompletionResponseDto>
    {
        public CreateCompletionCommand(string prompt, Guid? contextId = null)
        {
            Prompt = prompt;
            ContextId = contextId;
        }

        public string Prompt { get; set; }
        public Guid? ContextId { get; set; }       
    }
}
