using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;

public class MessageResponse
{
    /// <summary>
    /// Array of alternative model responses. The number of elements depends on the <c>n</c> parameter in the request,
    /// which specifies how many response variants the model should generate. Defaults to <c>n=1</c>.
    /// </summary>
    public Choice[]? Choices { get; set; }
}
