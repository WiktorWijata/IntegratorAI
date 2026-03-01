namespace IntegratorAI.Api.Contracts.Chat
{
    public class CompletionResponse
    {
        public string Id { get; set; }
        public CompletionMessage[] Messages { get; set; }
    }
}
