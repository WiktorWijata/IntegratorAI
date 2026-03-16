namespace IntegratorAI.Context.Domain;

public class Guardrail
{
    public long ToolId { get; protected set; }
    public string? Description { get; protected set; }
    public bool RequiresConfirmation { get; protected set; }
}
