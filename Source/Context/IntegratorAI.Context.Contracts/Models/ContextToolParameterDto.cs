namespace IntegratorAI.Context.Contracts.Models
{
    public class ContextToolParameterDto
    {
        public ContextToolParameterDto(string name, string type, string description)
        {
            Name = name;
            Type = type;
            Description = description;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
