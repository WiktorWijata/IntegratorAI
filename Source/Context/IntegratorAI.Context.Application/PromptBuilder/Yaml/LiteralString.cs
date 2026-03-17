namespace IntegratorAI.Context.Application.PromptBuilder.Yaml;

internal sealed class LiteralString(string value)
{
    public string Value { get; } = value;

    public static implicit operator LiteralString?(string? s) =>
        s is null ? null : new LiteralString(s);
}
