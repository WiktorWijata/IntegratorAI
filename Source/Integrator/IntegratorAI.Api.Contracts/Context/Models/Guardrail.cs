namespace IntegratorAI.Api.Contracts.Context.Models
{
    /// <summary>
    /// A safety constraint applied to a tool before it may be executed.
    /// </summary>
    public class Guardrail
    {
        /// <summary>
        /// Human-readable description of the constraint (e.g. "Do not delete production records").
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// When <c>true</c>, explicit user confirmation is required before the tool executes.
        /// </summary>
        public bool RequiresConfirmation { get; set; }
    }
}
