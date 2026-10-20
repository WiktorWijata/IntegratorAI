using System;

namespace IntegratorAI.Chat.Contracts.Models
{
    /// <summary>
    /// A single item of a streamed completion. The first item of a newly created completion
    /// carries the completion identifier, every following item carries a response token or
    /// a request of the model to run a tool.
    /// </summary>
    public class CompletionStreamEvent
    {
        private CompletionStreamEvent(CompletionStreamEventType type, string data, ToolCallDto toolCall = null)
        {
            Type = type;
            Data = data;
            ToolCall = toolCall;
        }

        public CompletionStreamEventType Type { get; }
        public string Data { get; }
        public ToolCallDto ToolCall { get; }

        public static CompletionStreamEvent ForCompletion(Guid completionId)
            => new CompletionStreamEvent(CompletionStreamEventType.Completion, completionId.ToString());

        public static CompletionStreamEvent ForToken(string token)
            => new CompletionStreamEvent(CompletionStreamEventType.Token, token);

        public static CompletionStreamEvent ForToolCall(ToolCallDto toolCall)
            => new CompletionStreamEvent(CompletionStreamEventType.ToolCall, null, toolCall);
    }

    public enum CompletionStreamEventType
    {
        Completion,
        Token,
        ToolCall
    }
}
