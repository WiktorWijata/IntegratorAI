using IntegratorAI.Providers.Domain;

namespace IntegratorAI.Providers.Contracts
{
    public class ProviderConfiguration
    {
        public ProviderType Type { get; set; }
        public required string BaseUrl { get; set; }
    }
}
