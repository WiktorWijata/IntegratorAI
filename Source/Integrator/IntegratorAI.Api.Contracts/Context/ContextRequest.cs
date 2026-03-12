using System.ComponentModel.DataAnnotations;
using IntegratorAI.Api.Contracts.Context.Models;

namespace IntegratorAI.Api.Contracts.Context
{
    public class ContextRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string SystemPrompt { get; set; }

        public ContextTool[] Tools { get; set; }
    }
}
