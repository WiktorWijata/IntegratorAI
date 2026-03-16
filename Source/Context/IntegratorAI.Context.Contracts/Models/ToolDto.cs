using System.Collections.Generic;

namespace IntegratorAI.Context.Contracts.Models
{
    public class ToolDto
    {
        public ToolDto(string name, string description, IEnumerable<ToolParameterDto> parameters = null)
        {
            Name = name;
            Description = description;
            Parameters = parameters;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<ToolParameterDto> Parameters { get; set; }
    }
}
