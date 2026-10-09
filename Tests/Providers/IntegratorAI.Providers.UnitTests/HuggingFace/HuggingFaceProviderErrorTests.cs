using System.Net;
using System.Text;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using NSubstitute;
using Refit;

namespace IntegratorAI.Providers.UnitTests.HuggingFace;

public class HuggingFaceProviderErrorTests
{
    private const string NoCreditsBody = """{"error":"You have no remaining credits."}""";

    private readonly IHuggingFaceApi _api = Substitute.For<IHuggingFaceApi>();
    private readonly HuggingFaceProvider _provider;

    public HuggingFaceProviderErrorTests()
    {
        _provider = new HuggingFaceProvider(_api) { PrimaryModel = "primary-model", SummarizationModel = string.Empty };
    }

    private static ProviderCompletionDto Completion()
        => new() { Messages = [new ProviderMessageDto { Role = "user", Content = "hi" }] };

    private static HttpResponseMessage Response(HttpStatusCode status, string body, string contentType = "application/json")
        => new(status) { Content = new StringContent(body, Encoding.UTF8, contentType) };

    private static async Task<List<string>> ReadAllAsync(IAsyncEnumerable<string> tokens)
    {
        var all = new List<string>();
        await foreach (var token in tokens)
        {
            all.Add(token);
        }

        return all;
    }

    [Fact]
    public async Task Stream_reads_the_tokens_of_a_successful_response()
    {
        const string sse = "data: {\"choices\":[{\"delta\":{\"content\":\"Hel\"}}]}\n\n"
                           + "data: {\"choices\":[{\"delta\":{\"content\":\"lo\"}}]}\n\n"
                           + "data: [DONE]\n\n";
        _api.StreamChatAsync(Arg.Any<MessageRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));

        var tokens = await ReadAllAsync(_provider.StreamCompletionAsync(Completion()));

        Assert.Equal(["Hel", "lo"], tokens);
    }

    [Fact]
    public async Task Stream_fails_with_a_provider_exception_instead_of_returning_an_empty_answer()
    {
        _api.StreamChatAsync(Arg.Any<MessageRequest>()).Returns(Response(HttpStatusCode.PaymentRequired, NoCreditsBody));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => ReadAllAsync(_provider.StreamCompletionAsync(Completion())));

        Assert.Equal(402, exception.ProviderStatusCode);
        Assert.Contains("no remaining credits", exception.Message);
    }

    [Fact]
    public async Task Stream_failure_happens_before_the_first_token_is_produced()
    {
        _api.StreamChatAsync(Arg.Any<MessageRequest>()).Returns(Response(HttpStatusCode.InternalServerError, "{}"));
        await using var enumerator = _provider.StreamCompletionAsync(Completion()).GetAsyncEnumerator();

        await Assert.ThrowsAsync<ProviderException>(async () => await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task Stream_wraps_a_connection_failure()
    {
        _api.StreamChatAsync(Arg.Any<MessageRequest>()).Returns(Task.FromException<HttpResponseMessage>(new HttpRequestException("no route")));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => ReadAllAsync(_provider.StreamCompletionAsync(Completion())));

        Assert.Null(exception.ProviderStatusCode);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task Completion_wraps_a_rejected_request_with_the_provider_status()
    {
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://router.example/v1/chat/completions"),
            HttpMethod.Post,
            Response(HttpStatusCode.PaymentRequired, NoCreditsBody),
            new RefitSettings());
        _api.ChatAsync(Arg.Any<MessageRequest>()).Returns(Task.FromException<MessageResponse>(apiException));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => _provider.CompletionAsync(Completion()));

        Assert.Equal(402, exception.ProviderStatusCode);
        Assert.Same(apiException, exception.InnerException);
    }

    [Fact]
    public async Task Summary_wraps_a_rejected_request_too()
    {
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://router.example/v1/chat/completions"),
            HttpMethod.Post,
            Response(HttpStatusCode.TooManyRequests, "{}"),
            new RefitSettings());
        _api.ChatAsync(Arg.Any<MessageRequest>()).Returns(Task.FromException<MessageResponse>(apiException));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => _provider.SummaryCompletionAsync(Completion()));

        Assert.Equal(429, exception.ProviderStatusCode);
    }
}
