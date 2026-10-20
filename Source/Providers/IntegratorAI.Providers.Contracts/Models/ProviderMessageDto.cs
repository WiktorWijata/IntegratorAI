namespace IntegratorAI.Providers.Contracts.Models
{
    public class ProviderMessageDto
    {
        public string Role { get; set; }
        public string Content { get; set; }

        /// <summary>Set on an assistant message that asks for tools instead of answering.</summary>
        public ProviderToolCallDto[] ToolCalls { get; set; }

        /// <summary>Set on a tool message: the call the content answers.</summary>
        public string ToolCallId { get; set; }
    }
}
