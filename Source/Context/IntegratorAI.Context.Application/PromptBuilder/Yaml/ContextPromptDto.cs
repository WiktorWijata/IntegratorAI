using YamlDotNet.Serialization;

namespace IntegratorAI.Context.Application.PromptBuilder.Yaml;

internal sealed class ContextPromptDto
{
    [YamlMember(Alias = "SYSTEM_ROLE")]
    public LiteralString? SystemRole { get; set; }

    [YamlMember(Alias = "DOMAIN_CONTEXT")]
    public LiteralString? DomainContext { get; set; }

    [YamlMember(Alias = "DECISION_POLICY")]
    public LiteralString? DecisionPolicy { get; set; }

    [YamlMember(Alias = "OPERATING_RULES")]
    public LiteralString? OperatingRules { get; set; }

    [YamlMember(Alias = "TOOLS")]
    public List<ToolDto>? Tools { get; set; }

    [YamlMember(Alias = "OUTPUT_FORMAT")]
    public LiteralString? OutputFormat { get; set; }
}
