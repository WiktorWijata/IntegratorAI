using IntegratorAI.Context.Domain;

namespace IntegratorAI.Context.UnitTests.Domain;

public class ContextTests
{
    [Fact]
    public void Constructor_InitializesEmptyCollections_WhenToolsAndExamplesAreNull()
    {
        var context = new Context.Domain.Context("name", "system role", null, null, null, null, null, null);

        Assert.Empty(context.Tools);
        Assert.Empty(context.Examples);
    }

    [Fact]
    public void Constructor_AddsAllTools_AndExamples()
    {
        var tools = new List<Tool>
        {
            new Tool("tool1", "desc1", null, null),
            new Tool("tool2", "desc2", null, null)
        };
        var examples = new List<Example>
        {
            new Example("input", "expected response")
        };

        var context = new Context.Domain.Context("name", "system role", null, null, null, null, tools, examples);

        Assert.Equal(2, context.Tools.Count);
        Assert.Single(context.Examples);
    }

    [Fact]
    public void AddTool_ThrowsInvalidOperationException_WhenToolWithSameNameExists()
    {
        var context = new Context.Domain.Context("name", "system role", null, null, null, null, null, null);
        context.AddTool(new Tool("duplicate", "desc1", null, null));

        Assert.Throws<InvalidOperationException>(() =>
            context.AddTool(new Tool("duplicate", "desc2", null, null)));
    }

    [Fact]
    public void AddTool_AddsToolSuccessfully()
    {
        var context = new Context.Domain.Context("name", "system role", null, null, null, null, null, null);

        context.AddTool(new Tool("tool1", "description", null, null));

        Assert.Single(context.Tools);
        Assert.Equal("tool1", context.Tools.First().Name);
    }
}
