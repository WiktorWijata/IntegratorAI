using System.ComponentModel.DataAnnotations;

namespace IntegratorAI.Api.Contracts.Context.Models
{
    public class ToolParameter
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
