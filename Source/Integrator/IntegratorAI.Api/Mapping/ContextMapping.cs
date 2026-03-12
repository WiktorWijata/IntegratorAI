using IntegratorAI.Api.Contracts.Context.Models;
using IntegratorAI.Context.Contracts.Models;

namespace IntegratorAI.Api.Mapping;

public static class ContextMapping
{
    public static ContextToolDto ToDto(this ContextTool contextTool)
    {
        return new ContextToolDto(
            name: contextTool.Name,
            description: contextTool.Description,
            parameters: contextTool.Parameters.Select(p => p.ToDto()));
    }

    public static ContextToolParameterDto ToDto(this ContextToolParameter parameter)
    {
        return new ContextToolParameterDto(parameter.Name, parameter.Type, parameter.Description);
    }
}
