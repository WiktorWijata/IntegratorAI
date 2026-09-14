using System.ComponentModel.DataAnnotations;
using IntegratorAI.Api.Contracts.Context.Models;

namespace IntegratorAI.Api.Contracts.Context
{
    /// <summary>
    /// Request body for creating a new AI context that defines the assistant's behavior.
    /// </summary>
    public class ContextRequest
    {
        /// <summary>
        /// Display name identifying the context.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Description of the assistant's role and persona (e.g. "You are a helpful customer support agent").
        /// </summary>
        [Required]
        public string SystemRole { get; set; }

        /// <summary>
        /// Background domain knowledge the assistant should be aware of.
        /// </summary>
        public string DomainContext { get; set; }

        /// <summary>
        /// Rules governing how the assistant should make decisions.
        /// </summary>
        public string DecisionPolicy { get; set; }

        /// <summary>
        /// Operational constraints and guidelines the assistant must follow.
        /// </summary>
        public string OperatingRules { get; set; }

        /// <summary>
        /// Preferred format for the assistant's responses (e.g. JSON, Markdown, plain text).
        /// </summary>
        public string OutputFormat { get; set; }

        /// <summary>
        /// External tools the assistant can invoke during a conversation.
        /// </summary>
        public Tool[] Tools { get; set; }

        /// <summary>
        /// Sample interactions demonstrating the expected assistant behavior.
        /// </summary>
        public Example[] Examples { get; set; }
    }
}
