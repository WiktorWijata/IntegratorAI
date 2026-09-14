namespace IntegratorAI.Api.Contracts.Chat.Models
{
    /// <summary>
    /// Represents a single message in a conversation.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Role of the message author. Possible values: <c>User</c>, <c>Assistant</c>, <c>System</c>.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Text content of the message.
        /// </summary>
        public string Content { get; set; }
    }
}
