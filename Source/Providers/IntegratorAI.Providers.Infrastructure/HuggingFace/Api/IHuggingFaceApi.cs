using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using Refit;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api;

public interface IHuggingFaceApi
{
    [Post("/v1/chat/completions")]
    Task<MessageResponse> ChatAsync([Body] MessageRequest request);

    [Post("/{modelId}")]
    Task<T> PipelineAsync<T>([AliasAs("modelId")] string modelId, [Body] PipelineRequest request);
}
