using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using Refit;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api;

/// <summary>
/// OpenRouter speaks the OpenAI chat completions format, the same as the HuggingFace router, so the requests and
/// responses are the ones already used for HuggingFace.
/// </summary>
public interface IOpenRouterApi
{
    [Post("/v1/chat/completions")]
    Task<MessageResponse> ChatAsync([Body] MessageRequest request);

    [Post("/v1/chat/completions")]
    Task<HttpResponseMessage> StreamChatAsync([Body] MessageRequest request);
}
