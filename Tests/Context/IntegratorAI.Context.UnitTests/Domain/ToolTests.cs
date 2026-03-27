using IntegratorAI.Context.Domain;

namespace IntegratorAI.Context.UnitTests.Domain;

public class ToolTests
{
    [Fact]
    public void Constructor_InitializesEmptyCollections_WhenParametersAndGuardrailsAreNull()
    {
        var tool = new Tool("name", "description", null, null);

        Assert.Empty(tool.Parameters);
        Assert.Empty(tool.Guardrails);
    }

    [Fact]
    public void AddParameter_ThrowsInvalidOperationException_WhenParameterWithSameNameExists()
    {
        var tool = new Tool("name", "description", null, null);
        tool.AddParameter(new ToolParameter { Name = "param1", Type = "string", Description = "desc" });

        Assert.Throws<InvalidOperationException>(() =>
            tool.AddParameter(new ToolParameter { Name = "param1", Type = "int", Description = "desc2" }));
    }

    [Fact]
    public void AddParameter_AddsParameterSuccessfully()
    {
        var tool = new Tool("name", "description", null, null);

        tool.AddParameter(new ToolParameter { Name = "param1", Type = "string", Description = "desc" });

        Assert.Single(tool.Parameters);
        Assert.Equal("param1", tool.Parameters.First().Name);
    }
}
