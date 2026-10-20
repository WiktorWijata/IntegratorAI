namespace IntegratorAI.Providers.Contracts.Models
{
    /// <summary>
    /// One item of a streamed answer of a provider: a piece of text or a request to run a tool.
    /// </summary>
    public class ProviderStreamEvent
    {
        private ProviderStreamEvent(string token, ProviderToolCallDto toolCall)
        {
            Token = token;
            ToolCall = toolCall;
        }

        public string Token { get; }
        public ProviderToolCallDto ToolCall { get; }
        public bool IsToolCall => ToolCall != null;

        public static ProviderStreamEvent ForToken(string token) => new ProviderStreamEvent(token, null);

        public static ProviderStreamEvent ForToolCall(ProviderToolCallDto toolCall) => new ProviderStreamEvent(null, toolCall);
    }
}
