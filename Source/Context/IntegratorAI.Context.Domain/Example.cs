namespace IntegratorAI.Context.Domain;

public class Example
{
    public long Id { get; set; }
    public Guid ContextId { get; set; }
    public string Input { get; protected set; } = null!; 
    public string ExpectedResponse { get; protected set; } = null!;

    public Example(string input, string expectedResponse)
    {
        Input = input;
        ExpectedResponse = expectedResponse;
    }

    protected Example() { }
}
