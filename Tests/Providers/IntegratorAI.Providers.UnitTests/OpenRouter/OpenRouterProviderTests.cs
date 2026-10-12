using System.Net;
using System.Text;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using IntegratorAI.Providers.Infrastructure.OpenRouter;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api;
using NSubstitute;
using Refit;

namespace IntegratorAI.Providers.UnitTests.OpenRouter;

public class OpenRouterProviderTests
{
    private const string NoCreditsBody = """{"error":{"code":402,"message":"Insufficient credits"}}""";

    private readonly IOpenRouterApi _api = Substitute.For<IOpenRouterApi>();
    private readonly OpenRouterProvider _provider;

    public OpenRouterProviderTests()
    {
        _provider = new OpenRouterProvider(_api) { PrimaryModel = "primary-model", SummarizationModel = string.Empty };
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

    private static MessageResponse Answer(string content)
        => new() { Choices = [new Choice { Message = new Message { Role = "assistant", Content = content } }] };

    [Fact]
    public async Task Completion_asks_the_primary_model_and_returns_the_answer()
    {
        _api.ChatAsync(Arg.Any<MessageRequest>()).Returns(Answer("hello"));

        var message = await _provider.CompletionAsync(Completion());

        Assert.Equal("hello", message.Content);
        await _api.Received(1).ChatAsync(Arg.Is<MessageRequest>(r => r.Model == "primary-model" && !r.Stream));
    }

    [Fact]
    public async Task Stream_reads_the_tokens_and_skips_the_keep_alive_comments()
    {
        const string sse = ": OPENROUTER PROCESSING\n\n"
                           + "data: {\"choices\":[{\"delta\":{\"content\":\"Hel\"}}]}\n\n"
                           + ": OPENROUTER PROCESSING\n\n"
                           + "data: {\"choices\":[{\"delta\":{\"content\":\"lo\"}}]}\n\n"
                           + "data: {\"choices\":[],\"usage\":{\"total_tokens\":5}}\n\n"
                           + "data: [DONE]\n\n";
        _api.StreamChatAsync(Arg.Any<MessageRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));

        var tokens = await ReadAllAsync(_provider.StreamCompletionAsync(Completion()));

        Assert.Equal(["Hel", "lo"], tokens);
        await _api.Received(1).StreamChatAsync(Arg.Is<MessageRequest>(r => r.Model == "primary-model" && r.Stream));
    }

    [Fact]
    public async Task Stream_fails_when_OpenRouter_reports_an_error_after_the_answer_has_started()
    {
        const string sse = "data: {\"choices\":[{\"delta\":{\"content\":\"Half\"}}]}\n\n"
                           + "data: {\"error\":{\"code\":\"server_error\",\"message\":\"Provider disconnected\"}}\n\n";
        _api.StreamChatAsync(Arg.Any<MessageRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));
        var tokens = new List<string>();

        var exception = await Assert.ThrowsAsync<ProviderException>(async () =>
        {
            await foreach (var token in _provider.StreamCompletionAsync(Completion()))
            {
                tokens.Add(token);
            }
        });

        Assert.Equal(["Half"], tokens);
        Assert.Contains("Provider disconnected", exception.Message);
    }

    [Fact]
    public async Task Stream_fails_with_a_provider_exception_instead_of_returning_an_empty_answer()
    {
        _api.StreamChatAsync(Arg.Any<MessageRequest>()).Returns(Response(HttpStatusCode.PaymentRequired, NoCreditsBody));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => ReadAllAsync(_provider.StreamCompletionAsync(Completion())));

        Assert.Equal(402, exception.ProviderStatusCode);
        Assert.Contains("Insufficient credits", exception.Message);
        Assert.Contains("OpenRouter", exception.Message);
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
            new HttpRequestMessage(HttpMethod.Post, "https://openrouter.example/api/v1/chat/completions"),
            HttpMethod.Post,
            Response(HttpStatusCode.PaymentRequired, NoCreditsBody),
            new RefitSettings());
        _api.ChatAsync(Arg.Any<MessageRequest>()).Returns(Task.FromException<MessageResponse>(apiException));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => _provider.CompletionAsync(Completion()));

        Assert.Equal(402, exception.ProviderStatusCode);
        Assert.Same(apiException, exception.InnerException);
    }

    [Fact]
    public async Task Summary_uses_the_primary_model_when_there_is_no_summarization_model()
    {
        _api.ChatAsync(Arg.Any<MessageRequest>()).Returns(Answer("short"));

        var message = await _provider.SummaryCompletionAsync(Completion());

        Assert.Equal("short", message.Content);
        await _api.Received(1).ChatAsync(Arg.Is<MessageRequest>(r => r.Model == "primary-model" && r.Messages.Length == 2));
    }

    [Fact]
    public async Task Summary_uses_the_summarization_model_when_there_is_one()
    {
        _provider.SummarizationModel = "cheap-model";
        _api.ChatAsync(Arg.Any<MessageRequest>()).Returns(Answer("short"));

        await _provider.SummaryCompletionAsync(Completion());

        await _api.Received(1).ChatAsync(Arg.Is<MessageRequest>(r => r.Model == "cheap-model"));
    }

    [Fact]
    public async Task Summary_wraps_a_rejected_request_too()
    {
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://openrouter.example/api/v1/chat/completions"),
            HttpMethod.Post,
            Response(HttpStatusCode.TooManyRequests, "{}"),
            new RefitSettings());
        _api.ChatAsync(Arg.Any<MessageRequest>()).Returns(Task.FromException<MessageResponse>(apiException));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => _provider.SummaryCompletionAsync(Completion()));

        Assert.Equal(429, exception.ProviderStatusCode);
    }
}
