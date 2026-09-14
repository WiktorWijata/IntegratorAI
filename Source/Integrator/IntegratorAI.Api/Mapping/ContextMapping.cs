using IntegratorAI.Api.Contracts.Context.Models;
using IntegratorAI.Context.Contracts.Models;

namespace IntegratorAI.Api.Mapping;

public static class ContextMapping
{
    extension(Tool contextTool)
    {
        public ToolDto ToDto()
        {
            return new ToolDto(
                name: contextTool.Name,
                description: contextTool.Description,
                parameters: contextTool.Parameters.Select(p => p.ToDto()),
                guardrails: contextTool.Guardrails.Select(g => g.ToDto()));
        }
    }

    extension(Example example)
    {
        public ExampleDto ToDto()
        {
            return new ExampleDto(
                input: example.Input,
                expectedResponse: example.expectedResponse);
        }
    }

    extension(ToolParameter parameter)
    {
        public ToolParameterDto ToDto()
        {
            return new ToolParameterDto(parameter.Name, parameter.Type, parameter.Description);
        }
    }

    extension(Guardrail guardrail)
    {
        public GuardrailDto ToDto()
        {
            return new GuardrailDto(guardrail.Description, guardrail.RequiresConfirmation);
        }
    }
}
