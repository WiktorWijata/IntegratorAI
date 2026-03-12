using System.ComponentModel.DataAnnotations;

namespace IntegratorAI.Api.Contracts.Context.Models
{
    public class ContextTool
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public ContextToolParameter[] Parameters { get; set; }
    }
}
