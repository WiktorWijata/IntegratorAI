namespace IntegratorAI.Context.Domain;

public class ToolParameter
{
    public long Id { get; set; }
    public long ToolId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Description { get; set; } = null!;
}
