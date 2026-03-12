using System.Collections.Generic;

namespace IntegratorAI.Context.Contracts.Models
{
    public class ContextToolDto
    {
        public ContextToolDto(string name, string description, IEnumerable<ContextToolParameterDto> parameters = null)
        {
            Name = name;
            Description = description;
            Parameters = parameters;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<ContextToolParameterDto> Parameters { get; set; }
    }
}
