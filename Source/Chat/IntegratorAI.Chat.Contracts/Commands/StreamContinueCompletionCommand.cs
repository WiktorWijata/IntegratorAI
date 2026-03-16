using System;
using MediatR;

namespace IntegratorAI.Chat.Contracts.Commands
{
    public class StreamContinueCompletionCommand : IStreamRequest<string>
    {
        public StreamContinueCompletionCommand(Guid completionId, string prompt)
        {
            CompletionId = completionId;
            Prompt = prompt;
        }

        public Guid CompletionId { get; }
        public string Prompt { get; }
    }
}
