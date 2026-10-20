using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Requests;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Responses;
using Refit;

namespace IntegratorAI.Providers.Infrastructure.OpenRouter.Api;

/// <summary>
/// OpenRouter speaks the OpenAI chat completions format, including tool calls.
/// </summary>
public interface IOpenRouterApi
{
    [Post("/v1/chat/completions")]
    Task<ChatResponse> ChatAsync([Body] ChatRequest request);

    [Post("/v1/chat/completions")]
    Task<HttpResponseMessage> StreamChatAsync([Body] ChatRequest request);
}
