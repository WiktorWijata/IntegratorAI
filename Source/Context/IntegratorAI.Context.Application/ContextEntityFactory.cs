using IntegratorAI.Context.Contracts.Models;
using IntegratorAI.Context.Domain;

namespace IntegratorAI.Context.Application;

internal static class ContextEntityFactory
{
    public static List<Tool>? CreateTools(IEnumerable<ToolDto>? tools)
        => tools?.Select(t => new Tool(
            name: t.Name,
            description: t.Description,
            parameters: t.Parameters?.Select(p => new ToolParameter
            {
                Name = p.Name,
                Type = p.Type,
                Description = p.Description
            }).ToList(),
            guardrails: t.Guardrails?.Select(g => new Guardrail(
                description: g.Description,
                requiresConfirmation: g.RequiresConfirmation)).ToList()
        )).ToList();

    public static List<Example>? CreateExamples(IEnumerable<ExampleDto>? examples)
        => examples?.Select(e => new Example(
            input: e.Input,
            expectedResponse: e.ExpectedResponse
        )).ToList();
}
