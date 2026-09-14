using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.Commands;

public class CreateCompletionCommand : IRequest<CompletionDto>
{
    public CreateCompletionCommand(string prompt, Guid? contextId = null)
    {
        Prompt = prompt;
        ContextId = contextId;
    }

    public string Prompt { get; }
    public Guid? ContextId { get; }
}
