using YamlDotNet.Serialization;

namespace IntegratorAI.Context.Application.PromptBuilder.Yaml;

internal sealed class ToolParameterDto
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = null!;

    [YamlMember(Alias = "type")]
    public string Type { get; set; } = null!;

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = null!;
}
