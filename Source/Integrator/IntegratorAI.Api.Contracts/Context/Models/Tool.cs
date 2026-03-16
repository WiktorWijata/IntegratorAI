using System.ComponentModel.DataAnnotations;

namespace IntegratorAI.Api.Contracts.Context.Models
{
    public class Tool
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public ToolParameter[] Parameters { get; set; }
    }
}
