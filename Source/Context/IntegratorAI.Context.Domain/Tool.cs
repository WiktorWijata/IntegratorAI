namespace IntegratorAI.Context.Domain;

public class Tool
{
    public int Id { get; set; }
    public Guid ContextId { get; set; }
    public string Name { get; protected set; } = null!;
    public string Description { get; protected set; } = null!;
    public ICollection<ToolParameter> Parameters { get; set; }

    public Tool(string name, string description, ICollection<ToolParameter>? parameters)
    {
        Name = name;
        Description = description;
        Parameters = new HashSet<ToolParameter>();

        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                AddParameter(parameter);
            }
        }
    }

    protected Tool()
    {
        Parameters = new HashSet<ToolParameter>();
    }

    public void AddParameter(ToolParameter parameter)
    {
        if (Parameters.Any(p => p.Name == parameter.Name))
        {
            throw new InvalidOperationException($"A parameter with the name '{parameter.Name}' already exists.");
        }

        Parameters.Add(parameter);
    }
}
