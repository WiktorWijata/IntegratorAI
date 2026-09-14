namespace IntegratorAI.Context.Domain;

public class Guardrail
{
    public long Id { get; set; }
    public long ToolId { get; protected set; }
    public string? Description { get; protected set; }
    public bool RequiresConfirmation { get; protected set; }

    public Guardrail(string? description, bool requiresConfirmation)
    {
        Description = description;
        RequiresConfirmation = requiresConfirmation;
    }

    protected Guardrail() { }
}
