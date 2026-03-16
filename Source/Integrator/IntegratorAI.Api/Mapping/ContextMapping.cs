using IntegratorAI.Api.Contracts.Context.Models;
using IntegratorAI.Context.Contracts.Models;

namespace IntegratorAI.Api.Mapping;

public static class ContextMapping
{
    public static ToolDto ToDto(this Tool contextTool)
    {
        return new ToolDto(
            name: contextTool.Name,
            description: contextTool.Description,
            parameters: contextTool.Parameters.Select(p => p.ToDto()));
    }

    public static ToolParameterDto ToDto(this ToolParameter parameter)
    {
        return new ToolParameterDto(parameter.Name, parameter.Type, parameter.Description);
    }
}
