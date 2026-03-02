namespace IntegratorAI.Providers.Domain;

public class Provider
{
    public int Id { get; set; }
    public ProviderType Type { get; set; }
    public required string PrimaryModel { get; set; }
    public string? SummarizationModel { get; set; }
    public bool IsActive { get; set; }
}
