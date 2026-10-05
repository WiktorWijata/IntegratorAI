using System;

namespace IntegratorAI.Chat.Contracts.Models
{
    /// <summary>
    /// A single item of a streamed completion. The first item of a newly created completion
    /// carries the completion identifier, every following item carries a response token.
    /// </summary>
    public class CompletionStreamEvent
    {
        private CompletionStreamEvent(CompletionStreamEventType type, string data)
        {
            Type = type;
            Data = data;
        }

        public CompletionStreamEventType Type { get; }
        public string Data { get; }

        public static CompletionStreamEvent ForCompletion(Guid completionId)
            => new CompletionStreamEvent(CompletionStreamEventType.Completion, completionId.ToString());

        public static CompletionStreamEvent ForToken(string token)
            => new CompletionStreamEvent(CompletionStreamEventType.Token, token);
    }

    public enum CompletionStreamEventType
    {
        Completion,
        Token
    }
}
