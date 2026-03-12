using IntegratorAI.BuildingBlocks.Domain;

namespace IntegratorAI.Context.Domain;

public class Context : AggregateRoot<Guid>
{
    public override Guid Id { get; protected set; }
    public string Name { get; protected set; }
    public string SystemPrompt { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public ICollection<Tool> Tools { get; protected set; }
    
    public Context(string name, string systemPrompt, ICollection<Tool>? tools)
    {
        CreatedAt = DateTime.UtcNow;        
        Name = name;
        SystemPrompt = systemPrompt;
        Tools = new HashSet<Tool>();

        if (tools is not null)
        {
            foreach (var tool in tools)
            {
                AddTool(tool);
            }
        }
    }

    protected Context()
    {
        Tools = new HashSet<Tool>();
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
