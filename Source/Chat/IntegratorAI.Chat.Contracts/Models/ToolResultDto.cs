namespace IntegratorAI.Chat.Contracts.Models
{
    /// <summary>
    /// The outcome of one tool call, sent back by the client that ran the tool.
    /// </summary>
    public class ToolResultDto
    {
        public string ToolCallId { get; set; }
        public string Content { get; set; }

        /// <summary>True when the tool failed; the content then says why.</summary>
        public bool IsError { get; set; }
    }
}
