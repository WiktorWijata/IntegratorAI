namespace IntegratorAI.Chat.Contracts.Models
{
    /// <summary>
    /// A request of the model to run a tool. <see cref="Arguments"/> is a JSON object.
    /// </summary>
    public class ToolCallDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Arguments { get; set; }
    }
}
