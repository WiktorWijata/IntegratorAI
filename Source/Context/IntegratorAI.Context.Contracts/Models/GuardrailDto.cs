namespace IntegratorAI.Context.Contracts.Models
{
    public class GuardrailDto
    {
        public GuardrailDto(string description, bool requiresConfirmation)
        {
            Description = description;
            RequiresConfirmation = requiresConfirmation;
        }

        public string Description { get; set; }
        public bool RequiresConfirmation { get; set; }
    }
}
