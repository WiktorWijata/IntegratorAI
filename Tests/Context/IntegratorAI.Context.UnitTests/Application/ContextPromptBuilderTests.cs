using IntegratorAI.Context.Application.PromptBuilder;
using IntegratorAI.Context.Domain;

namespace IntegratorAI.Context.UnitTests.Application;

public class ContextPromptBuilderTests
{
    private static Context.Domain.Context BuildContext(
        string systemRole = "You are a helpful assistant",
        string? domainContext = null,
        string? decisionPolicy = null,
        string? operatingRules = null,
        string? outputFormat = null,
        ICollection<Tool>? tools = null,
        ICollection<Example>? examples = null)
        => new Context.Domain.Context("test", systemRole, domainContext, decisionPolicy, operatingRules, outputFormat, tools, examples);

    [Fact]
    public void ToYaml_IncludesSystemRole()
    {
        var context = BuildContext(systemRole: "You are a helpful assistant");

        var yaml = context.ToYaml();

        Assert.Contains("SYSTEM_ROLE", yaml);
        Assert.Contains("You are a helpful assistant", yaml);
    }

    [Fact]
    public void ToYaml_OmitsOptionalFields_WhenNull()
    {
        var context = BuildContext();

        var yaml = context.ToYaml();

        Assert.DoesNotContain("DOMAIN_CONTEXT", yaml);
        Assert.DoesNotContain("DECISION_POLICY", yaml);
        Assert.DoesNotContain("OPERATING_RULES", yaml);
        Assert.DoesNotContain("OUTPUT_FORMAT", yaml);
        Assert.DoesNotContain("TOOLS", yaml);
        Assert.DoesNotContain("EXAMPLES", yaml);
    }

    [Fact]
    public void ToYaml_OmitsTools_WhenContextHasNoTools()
    {
        var context = BuildContext(tools: []);

        var yaml = context.ToYaml();

        Assert.DoesNotContain("TOOLS", yaml);
    }

    [Fact]
    public void ToYaml_IncludesTools_WhenPresent_WithNameAndDescription()
    {
        var tools = new List<Tool> { new Tool("search_web", "Searches the web", null, null) };
        var context = BuildContext(tools: tools);

        var yaml = context.ToYaml();

        Assert.Contains("TOOLS", yaml);
        Assert.Contains("search_web", yaml);
        Assert.Contains("Searches the web", yaml);
    }

    [Fact]
    public void ToYaml_OmitsToolParameters_WhenToolHasNone()
    {
        var tools = new List<Tool> { new Tool("tool1", "desc", null, null) };
        var context = BuildContext(tools: tools);

        var yaml = context.ToYaml();

        Assert.DoesNotContain("parameters", yaml);
    }

    [Fact]
    public void ToYaml_IncludesToolParameters_WhenPresent()
    {
        var parameters = new List<ToolParameter>
        {
            new ToolParameter { Name = "query", Type = "string", Description = "Search query" }
        };
        var tools = new List<Tool> { new Tool("search_web", "Searches the web", parameters, null) };
        var context = BuildContext(tools: tools);

        var yaml = context.ToYaml();

        Assert.Contains("parameters", yaml);
        Assert.Contains("query", yaml);
        Assert.Contains("string", yaml);
    }

    [Fact]
    public void ToYaml_OmitsExamples_WhenContextHasNone()
    {
        var context = BuildContext(examples: []);

        var yaml = context.ToYaml();

        Assert.DoesNotContain("EXAMPLES", yaml);
    }

    [Fact]
    public void ToYaml_IncludesExamples_WhenPresent_WithInputAndExpectedResponse()
    {
        var examples = new List<Example> { new Example("What is 2+2?", "4") };
        var context = BuildContext(examples: examples);

        var yaml = context.ToYaml();

        Assert.Contains("EXAMPLES", yaml);
        Assert.Contains("What is 2+2?", yaml);
        Assert.Contains("4", yaml);
    }

    [Fact]
    public void ToYaml_OmitsGuardrails_WhenToolHasNone()
    {
        var tools = new List<Tool> { new Tool("tool1", "desc", null, null) };
        var context = BuildContext(tools: tools);

        var yaml = context.ToYaml();

        Assert.DoesNotContain("guardrails", yaml);
    }

    [Fact]
    public void ToYaml_IncludesGuardrails_WhenPresent()
    {
        var guardrails = new List<Guardrail> { new Guardrail("Do not access restricted data", requiresConfirmation: true) };
        var tools = new List<Tool> { new Tool("tool1", "desc", null, guardrails) };
        var context = BuildContext(tools: tools);

        var yaml = context.ToYaml();

        Assert.Contains("guardrails", yaml);
        Assert.Contains("Do not access restricted data", yaml);
    }
}
