namespace IntegratorAI.Context.Contracts.Models
{
    public class ToolParameterDto
    {
        public ToolParameterDto(string name, string type, string description, bool required = false)
        {
            Name = name;
            Type = type;
            Description = description;
            Required = required;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
    }
}
