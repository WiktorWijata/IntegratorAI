namespace IntegratorAI.Context.Contracts.Models
{
    public class ExampleDto
    {
        public ExampleDto(string input, string expectedResponse)
        {
            Input = input;
            ExpectedResponse = expectedResponse;
        }

        public string Input { get; set; }
        public string ExpectedResponse { get; set; }
    }
}
