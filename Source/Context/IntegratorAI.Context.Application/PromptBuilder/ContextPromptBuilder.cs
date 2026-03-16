using IntegratorAI.Context.Application.PromptBuilder.Yaml;
using YamlDotNet.Serialization;
using ContextAggregate = IntegratorAI.Context.Domain.Context;

namespace IntegratorAI.Context.Application.PromptBuilder;

public static class ContextPromptBuilder
{
    private static readonly ISerializer _serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .WithTypeConverter(new LiteralStringConverter())
        .Build();

    public static string ToYaml(ContextAggregate context)
    {
        var dto = new ContextPromptDto
        {
            SystemRole = context.SystemRole,
            DomainContext = context.DomainContext,
            DecisionPolicy = context.DecisionPolicy,
            OperatingRules = context.OperatingRules,
            Tools = context.Tools.Count > 0
                ? context.Tools.Select(t => new ToolDto
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.Parameters.Count > 0
                        ? t.Parameters.Select(p => new ToolParameterDto
                        {
                            Name = p.Name,
                            Type = p.Type,
                            Description = p.Description
                        }).ToList()
                        : null
                }).ToList()
                : null,
            OutputFormat = context.OutputFormat
        };

        return _serializer.Serialize(dto).TrimEnd();
    }
}

