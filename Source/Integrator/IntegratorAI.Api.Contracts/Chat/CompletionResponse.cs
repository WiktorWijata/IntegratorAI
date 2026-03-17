using IntegratorAI.Api.Contracts.Chat.Models;

namespace IntegratorAI.Api.Contracts.Chat
{
    public class CompletionResponse
    {
        public string Id { get; set; }
        public Message[] Messages { get; set; }
    }
}
