using YamlDotNet.Serialization;

namespace IntegratorAI.Context.Application.PromptBuilder.Yaml;

internal sealed class ToolDto
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = null!;

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = null!;

    [YamlMember(Alias = "parameters")]
    public List<ToolParameterDto>? Parameters { get; set; }

    [YamlMember(Alias = "guardrails")]
    public List<GuardrailDto>? Guardrails { get; set; }
}
