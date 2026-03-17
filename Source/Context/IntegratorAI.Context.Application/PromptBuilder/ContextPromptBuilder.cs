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

    extension(ContextAggregate context)
    {
        public string ToYaml()
        {
            var dto = new ContextPromptDto
            {
                SystemRole = context.SystemRole,
                DomainContext = context.DomainContext,
                DecisionPolicy = context.DecisionPolicy,
                OperatingRules = context.OperatingRules,
                Tools = context.Tools.Count > 0
                    ? [.. context.Tools.Select(t => new ToolDto
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = t.Parameters.Count > 0
                            ? [.. t.Parameters.Select(p => new ToolParameterDto
                            {
                                Name = p.Name,
                                Type = p.Type,
                                Description = p.Description
                            })]
                            : null,
                        Guardrails = t.Guardrails.Count > 0
                            ? [.. t.Guardrails.Select(g => new GuardrailDto
                            {
                                Description = g.Description,
                                RequiresConfirmation = g.RequiresConfirmation
                            })]
                            : null
                    })]
                    : null,
                Examples = context.Examples.Count > 0
                    ? [.. context.Examples.Select(e => new ExampleDto
                    {
                        Input = e.Input,
                        ExpectedResponse = e.ExpectedResponse
                    })]
                    : null,
                OutputFormat = context.OutputFormat
            };

            return _serializer.Serialize(dto).TrimEnd();
        }
    }
}

