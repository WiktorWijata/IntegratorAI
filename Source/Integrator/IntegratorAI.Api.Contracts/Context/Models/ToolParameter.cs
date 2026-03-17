using System.ComponentModel.DataAnnotations;

namespace IntegratorAI.Api.Contracts.Context.Models
{
    /// <summary>
    /// Defines an input parameter accepted by a tool.
    /// </summary>
    public class ToolParameter
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Data type of the parameter (e.g. <c>string</c>, <c>integer</c>, <c>boolean</c>).
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// Description of what the parameter represents and how it should be populated.
        /// </summary>
        [Required]
        public string Description { get; set; }
    }
}
