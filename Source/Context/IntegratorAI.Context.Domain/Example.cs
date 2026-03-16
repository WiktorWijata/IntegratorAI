namespace IntegratorAI.Context.Domain;

public class Example
{
    public Guid ContextId { get; set; }
    public string UserInput { get; protected set; } = null!; 
    public string ExpectedAgentResponse { get; protected set; } = null!;
}
