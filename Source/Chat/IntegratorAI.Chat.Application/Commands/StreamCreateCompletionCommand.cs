using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.Commands;

public class StreamCreateCompletionCommand : IStreamRequest<CompletionStreamEvent>
{
    public StreamCreateCompletionCommand(string prompt, Guid? contextId = null)
    {
        Prompt = prompt;
        ContextId = contextId;
    }

    public string Prompt { get; }
    public Guid? ContextId { get; }
}
