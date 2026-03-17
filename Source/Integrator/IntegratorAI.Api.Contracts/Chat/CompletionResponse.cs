using IntegratorAI.Api.Contracts.Chat.Models;

namespace IntegratorAI.Api.Contracts.Chat
{
    /// <summary>
    /// Response containing the result of a completion request.
    /// </summary>
    public class CompletionResponse
    {
        /// <summary>
        /// Unique identifier of the completion.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Full conversation history including the latest assistant response.
        /// </summary>
        public Message[] Messages { get; set; }
    }
}
