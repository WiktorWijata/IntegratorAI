using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.Commands;

public class ContinueCompletionCommand : IRequest<CompletionDto>
{
    public ContinueCompletionCommand(Guid completionId, string prompt)
    {
        CompletionId = completionId;
        Prompt = prompt;
    }

    public Guid CompletionId { get; }
    public string Prompt { get; }
}
