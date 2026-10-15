using System.ComponentModel.DataAnnotations;

namespace IntegratorAI.Api.Contracts.Chat
{
    /// <summary>
    /// Request body for answering the tools the model asked for (the <c>tool_call</c> events of a stream).
    /// </summary>
    public class ToolResultsRequest
    {
        /// <summary>
        /// One result for every tool call of the last answer, no more and no fewer.
        /// </summary>
        [Required]
        [MinLength(1)]
        public ToolResultRequest[] Results { get; set; }
    }
}
