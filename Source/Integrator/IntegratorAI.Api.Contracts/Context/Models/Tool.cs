using System.ComponentModel.DataAnnotations;

namespace IntegratorAI.Api.Contracts.Context.Models
{
    /// <summary>
    /// Represents an external tool that the assistant can invoke during a conversation.
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// Unique name of the tool used to identify and invoke it.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Description of what the tool does and when the assistant should use it.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Input parameters required by the tool.
        /// </summary>
        public ToolParameter[] Parameters { get; set; }

        /// <summary>
        /// Safety constraints that must be satisfied before the tool can be executed.
        /// </summary>
        public Guardrail[] Guardrails { get; set; }
    }
}
