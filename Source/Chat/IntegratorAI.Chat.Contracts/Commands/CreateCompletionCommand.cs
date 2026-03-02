using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Contracts.Commands
{
    public class CreateCompletionCommand : IRequest<CompletionResponseDto>
    {
        public CreateCompletionCommand(string prompt)
        {
            Prompt = prompt;
        }

        public string Prompt { get; set; }
    }
}
