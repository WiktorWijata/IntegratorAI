using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using Refit;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api;

public interface IHuggingFaceApi
{
    [Post("/chat/completions")]
    public Task<MessageResponse> ChatAsync([Body] MessageRequest request);
}
