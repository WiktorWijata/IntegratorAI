namespace IntegratorAI.Providers.Contracts.Models
{
    public class ProviderCompletionDto
    {
        public ProviderMessageDto[] Messages { get; set; }

        /// <summary>The tools the model may ask for. A provider that cannot call tools ignores them.</summary>
        public ProviderToolDto[] Tools { get; set; }
    }
}
