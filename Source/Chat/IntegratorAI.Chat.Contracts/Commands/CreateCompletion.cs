using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Contracts.Commands
{
    public class CreateCompletion : IRequest<CompletionResponseDto>
    {
        public CreateCompletion(string prompt)
        {
            Prompt = prompt;
        }

        public string Prompt { get; set; }
    }
}
