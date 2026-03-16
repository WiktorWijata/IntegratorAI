using IntegratorAI.BuildingBlocks.Domain;

namespace IntegratorAI.Context.Domain;

public class Context : AggregateRoot<Guid>
{
    public override Guid Id { get; protected set; }
    public string Name { get; protected set; }
    public string SystemRole { get; protected set; }
    public string? DomainContext { get; protected set; }
    public string? DecisionPolicy { get; protected set; }
    public string? OperatingRules { get; protected set; }
    public string? OutputFormat { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public ICollection<Tool> Tools { get; protected set; }
    public ICollection<Example> Examples { get; protected set; }

    public Context(
        string name, 
        string systemRole, 
        string? domainContext, 
        string? decisionPolicy, 
        string? operatingRules, 
        string? outputFormat, 
        ICollection<Tool>? tools, 
        ICollection<Example>? examples)
    {
        CreatedAt = DateTime.UtcNow;
        Name = name;
        SystemRole = systemRole;
        DomainContext = domainContext;
        DecisionPolicy = decisionPolicy;
        OperatingRules = operatingRules;
        OutputFormat = outputFormat;
        Tools = new HashSet<Tool>();
        Examples = new HashSet<Example>();

        if (tools is not null)
        {
            foreach (var tool in tools)
            {
                AddTool(tool);
            }
        }

        if (examples is not null)
        {
            foreach (var example in examples)
            {
                Examples.Add(example);
            }
        }
    }

    protected Context()
    {
        Tools = new HashSet<Tool>();
        Examples = new HashSet<Example>();
    }

    public void AddTool(Tool tool)
    {
        if (Tools.Any(t => t.Name == tool.Name))
        {
            throw new InvalidOperationException($"A tool with the name '{tool.Name}' already exists.");
        }

        Tools.Add(tool);
    }
}