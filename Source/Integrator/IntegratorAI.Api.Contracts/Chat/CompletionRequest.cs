namespace IntegratorAI.Api.Contracts.Chat
{
    /// <summary>
    /// Request body for creating or continuing a completion.
    /// </summary>
    public class CompletionRequest
    {
        /// <summary>
        /// The user's message or question to send to the AI assistant.
        /// </summary>
        public string Prompt { get; set; }
    }
}
