namespace IntegratorAI.Chat.Contracts.Models
{
    public class CompletionDto
    {
        public string CompletionId { get; set; }
        public MessageDto[] Messages { get; set; }
    }
}
