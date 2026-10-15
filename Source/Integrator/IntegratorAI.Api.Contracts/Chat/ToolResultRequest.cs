using System.ComponentModel.DataAnnotations;

namespace IntegratorAI.Api.Contracts.Chat
{
    /// <summary>
    /// The outcome of one tool call.
    /// </summary>
    public class ToolResultRequest
    {
        /// <summary>
        /// Maximum length of <see cref="Content"/>.
        /// </summary>
        public const int MaxContentLength = 50000;

        /// <summary>
        /// Identifier of the tool call, as sent in the <c>tool_call</c> event.
        /// </summary>
        [Required]
        public string ToolCallId { get; set; }

        /// <summary>
        /// What the tool returned (usually JSON), or the reason it failed when <see cref="IsError"/> is set.
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        [MaxLength(MaxContentLength)]
        public string Content { get; set; }

        /// <summary>
        /// True when the tool failed.
        /// </summary>
        public bool IsError { get; set; }
    }
}
