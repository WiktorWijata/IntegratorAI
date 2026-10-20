namespace IntegratorAI.Providers.Contracts.Models
{
    /// <summary>
    /// A tool the model may ask for.
    /// </summary>
    public class ProviderToolDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProviderToolParameterDto[] Parameters { get; set; }
    }

    public class ProviderToolParameterDto
    {
        public string Name { get; set; }

        /// <summary>string, integer, number or boolean; anything else is sent as a string.</summary>
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
    }
}
