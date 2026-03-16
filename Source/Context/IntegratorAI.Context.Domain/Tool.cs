namespace IntegratorAI.Context.Domain;

public class Tool
{
    public long Id { get; set; }
    public Guid ContextId { get; set; }
    public string Name { get; protected set; } = null!;
    public string Description { get; protected set; } = null!;
    public ICollection<ToolParameter> Parameters { get; set; }
    public ICollection<Guardrail> Guardrails { get; set; }

    public Tool(string name, string description, ICollection<ToolParameter>? parameters)
    {
        Name = name;
        Description = description;
        Parameters = new HashSet<ToolParameter>();
        Guardrails = new HashSet<Guardrail>();

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
        Guardrails = new HashSet<Guardrail>();
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
