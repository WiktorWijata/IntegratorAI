using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;

public class MessageResponse
{
    public Choice[]? Choices { get; set; }
}
