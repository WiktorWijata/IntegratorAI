using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace;

public class HuggingFaceProvider : IProvider, IProviderInitalizable
{
    public string Model { get; set; }

    private readonly IHuggingFaceApi _huggingFaceApi;

    public HuggingFaceProvider(IHuggingFaceApi huggingFaceApi)
    {
        _huggingFaceApi = huggingFaceApi;
    }

    public async Task<CompletionDto> CompletionAsync(CompletionDto completion)
    {
        var request = completion.ToRequest(Model);
        var response = await _huggingFaceApi.ChatAsync(request);
        return response.ToDto();
    }
}
