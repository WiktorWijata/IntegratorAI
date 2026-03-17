using YamlDotNet.Serialization;

namespace IntegratorAI.Context.Application.PromptBuilder.Yaml;

internal sealed class GuardrailDto
{
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    [YamlMember(Alias = "requires_confirmation")]
    public bool RequiresConfirmation { get; set; }
}
