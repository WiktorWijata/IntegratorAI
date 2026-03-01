namespace IntegratorAI.Chat.Contracts.Models
{
    public class CompletionResponseDto
    {
        public string CompletionId { get; set; }
        public CompletionMessageDto[] Messages { get; set; }
    }
}
