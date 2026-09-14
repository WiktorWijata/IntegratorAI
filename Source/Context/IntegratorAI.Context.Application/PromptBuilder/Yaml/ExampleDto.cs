using YamlDotNet.Serialization;

namespace IntegratorAI.Context.Application.PromptBuilder.Yaml;

internal sealed class ExampleDto
{
    [YamlMember(Alias = "input")]
    public string Input { get; set; } = null!;

    [YamlMember(Alias = "expected_response")]
    public string ExpectedResponse { get; set; } = null!;
}
