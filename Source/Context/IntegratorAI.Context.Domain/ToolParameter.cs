namespace IntegratorAI.Context.Domain;

public class ToolParameter
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Description { get; set; } = null!;
}
