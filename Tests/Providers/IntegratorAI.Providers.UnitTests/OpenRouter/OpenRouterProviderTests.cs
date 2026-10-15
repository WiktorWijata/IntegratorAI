using System.Net;
using System.Text;
using System.Text.Json;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.OpenRouter;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Models;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Requests;
using IntegratorAI.Providers.Infrastructure.OpenRouter.Api.Responses;
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

    private static async Task<List<ProviderStreamEvent>> ReadAllAsync(IAsyncEnumerable<ProviderStreamEvent> events)
    {
        var all = new List<ProviderStreamEvent>();
        await foreach (var item in events)
        {
            all.Add(item);
        }

        return all;
    }

    private static async Task<List<string>> ReadTokensAsync(IAsyncEnumerable<ProviderStreamEvent> events)
        => (await ReadAllAsync(events)).Select(e => e.Token!).ToList();

    private static ChatResponse Answer(string content)
        => new() { Choices = [new ChatChoice { Message = new ChatMessage { Role = "assistant", Content = content } }] };

    private static string Chunk(string json) => $"data: {json}\n\n";

    [Fact]
    public async Task Completion_asks_the_primary_model_and_returns_the_answer()
    {
        _api.ChatAsync(Arg.Any<ChatRequest>()).Returns(Answer("hello"));

        var message = await _provider.CompletionAsync(Completion());

        Assert.Equal("hello", message.Content);
        await _api.Received(1).ChatAsync(Arg.Is<ChatRequest>(r => r!.Model == "primary-model" && !r.Stream));
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
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));

        var tokens = await ReadTokensAsync(_provider.StreamCompletionAsync(Completion()));

        Assert.Equal(["Hel", "lo"], tokens);
        await _api.Received(1).StreamChatAsync(Arg.Is<ChatRequest>(r => r!.Model == "primary-model" && r.Stream));
    }

    [Fact]
    public async Task Stream_fails_when_OpenRouter_reports_an_error_after_the_answer_has_started()
    {
        const string sse = "data: {\"choices\":[{\"delta\":{\"content\":\"Half\"}}]}\n\n"
                           + "data: {\"error\":{\"code\":\"server_error\",\"message\":\"Provider disconnected\"}}\n\n";
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));
        var tokens = new List<string>();

        var exception = await Assert.ThrowsAsync<ProviderException>(async () =>
        {
            await foreach (var item in _provider.StreamCompletionAsync(Completion()))
            {
                tokens.Add(item.Token!);
            }
        });

        Assert.Equal(["Half"], tokens);
        Assert.Contains("Provider disconnected", exception.Message);
    }

    [Fact]
    public async Task Stream_fails_with_a_provider_exception_instead_of_returning_an_empty_answer()
    {
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.PaymentRequired, NoCreditsBody));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => ReadAllAsync(_provider.StreamCompletionAsync(Completion())));

        Assert.Equal(402, exception.ProviderStatusCode);
        Assert.Contains("Insufficient credits", exception.Message);
        Assert.Contains("OpenRouter", exception.Message);
    }

    [Fact]
    public async Task Stream_failure_happens_before_the_first_token_is_produced()
    {
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.InternalServerError, "{}"));
        await using var enumerator = _provider.StreamCompletionAsync(Completion()).GetAsyncEnumerator();

        await Assert.ThrowsAsync<ProviderException>(async () => await enumerator.MoveNextAsync());
    }

    [Fact]
    public async Task Stream_wraps_a_connection_failure()
    {
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Task.FromException<HttpResponseMessage>(new HttpRequestException("no route")));

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
        _api.ChatAsync(Arg.Any<ChatRequest>()).Returns(Task.FromException<ChatResponse>(apiException));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => _provider.CompletionAsync(Completion()));

        Assert.Equal(402, exception.ProviderStatusCode);
        Assert.Same(apiException, exception.InnerException);
    }

    [Fact]
    public async Task Summary_uses_the_primary_model_when_there_is_no_summarization_model()
    {
        _api.ChatAsync(Arg.Any<ChatRequest>()).Returns(Answer("short"));

        var message = await _provider.SummaryCompletionAsync(Completion());

        Assert.Equal("short", message.Content);
        await _api.Received(1).ChatAsync(Arg.Is<ChatRequest>(r => r!.Model == "primary-model" && r.Messages.Length == 2));
    }

    [Fact]
    public async Task Summary_uses_the_summarization_model_when_there_is_one()
    {
        _provider.SummarizationModel = "cheap-model";
        _api.ChatAsync(Arg.Any<ChatRequest>()).Returns(Answer("short"));

        await _provider.SummaryCompletionAsync(Completion());

        await _api.Received(1).ChatAsync(Arg.Is<ChatRequest>(r => r!.Model == "cheap-model"));
    }

    [Fact]
    public async Task Summary_wraps_a_rejected_request_too()
    {
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://openrouter.example/api/v1/chat/completions"),
            HttpMethod.Post,
            Response(HttpStatusCode.TooManyRequests, "{}"),
            new RefitSettings());
        _api.ChatAsync(Arg.Any<ChatRequest>()).Returns(Task.FromException<ChatResponse>(apiException));

        var exception = await Assert.ThrowsAsync<ProviderException>(() => _provider.SummaryCompletionAsync(Completion()));

        Assert.Equal(429, exception.ProviderStatusCode);
    }

    [Fact]
    public async Task Summary_writes_tool_calls_and_results_down_as_plain_text()
    {
        var completion = new ProviderCompletionDto
        {
            Messages =
            [
                new ProviderMessageDto { Role = "User", Content = "about project 5" },
                new ProviderMessageDto
                {
                    Role = "Assistant",
                    Content = string.Empty,
                    ToolCalls = [new ProviderToolCallDto { Id = "c1", Name = "get_project", Arguments = "{\"id\":5}" }],
                },
                new ProviderMessageDto { Role = "Tool", Content = "{\"name\":\"X\"}", ToolCallId = "c1" },
            ],
        };
        _api.ChatAsync(Arg.Any<ChatRequest>()).Returns(Answer("short"));

        await _provider.SummaryCompletionAsync(completion);

        await _api.Received(1).ChatAsync(Arg.Is<ChatRequest>(r =>
            r!.Messages.All(m => m.ToolCalls == null && m.ToolCallId == null && m.Role != "tool")));
    }

    private static ProviderCompletionDto CompletionWithTool() => new()
    {
        Messages = [new ProviderMessageDto { Role = "user", Content = "project 5?" }],
        Tools =
        [
            new ProviderToolDto
            {
                Name = "get_project",
                Description = "Details of one project",
                Parameters =
                [
                    new ProviderToolParameterDto { Name = "id", Type = "long", Description = "Project id", Required = true },
                    new ProviderToolParameterDto { Name = "language", Type = "string", Description = "Language" },
                ],
            },
        ],
    };

    [Fact]
    public async Task Tools_are_sent_with_a_json_schema_and_tool_choice_auto()
    {
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, "data: [DONE]\n\n", "text/event-stream"));

        await ReadAllAsync(_provider.StreamCompletionAsync(CompletionWithTool()));

        var request = (ChatRequest)_api.ReceivedCalls().Single().GetArguments()[0]!;
        var json = JsonSerializer.Serialize(request);
        using var document = JsonDocument.Parse(json);
        var function = document.RootElement.GetProperty("tools")[0].GetProperty("function");

        Assert.Equal("auto", document.RootElement.GetProperty("tool_choice").GetString());
        Assert.Equal("get_project", function.GetProperty("name").GetString());
        var parameters = function.GetProperty("parameters");
        Assert.Equal("object", parameters.GetProperty("type").GetString());
        Assert.Equal("integer", parameters.GetProperty("properties").GetProperty("id").GetProperty("type").GetString());
        Assert.Equal("string", parameters.GetProperty("properties").GetProperty("language").GetProperty("type").GetString());
        Assert.Equal(["id"], parameters.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToArray());
    }

    [Fact]
    public async Task No_tools_means_no_tools_and_no_tool_choice_in_the_request()
    {
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, "data: [DONE]\n\n", "text/event-stream"));

        await ReadAllAsync(_provider.StreamCompletionAsync(Completion()));

        var request = (ChatRequest)_api.ReceivedCalls().Single().GetArguments()[0]!;
        var json = JsonSerializer.Serialize(request);

        Assert.DoesNotContain("tools", json);
        Assert.DoesNotContain("tool_choice", json);
    }

    [Fact]
    public async Task Stream_puts_a_tool_call_together_from_its_pieces()
    {
        var sse = Chunk("""{"choices":[{"delta":{"role":"assistant","content":null,"tool_calls":[{"index":0,"id":"call_1","type":"function","function":{"name":"get_project","arguments":""}}]}}]}""")
                  + Chunk("""{"choices":[{"delta":{"tool_calls":[{"index":0,"function":{"arguments":"{\"id\":"}}]}}]}""")
                  + Chunk("""{"choices":[{"delta":{"tool_calls":[{"index":0,"function":{"arguments":"5}"}}]}}]}""")
                  + Chunk("""{"choices":[{"delta":{},"finish_reason":"tool_calls"}]}""")
                  + "data: [DONE]\n\n";
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));

        var events = await ReadAllAsync(_provider.StreamCompletionAsync(CompletionWithTool()));

        var call = Assert.Single(events).ToolCall!;
        Assert.Equal("call_1", call.Id);
        Assert.Equal("get_project", call.Name);
        Assert.Equal("{\"id\":5}", call.Arguments);
    }

    [Fact]
    public async Task Stream_hands_over_several_tool_calls_in_order_and_defaults_empty_arguments()
    {
        var sse = Chunk("""{"choices":[{"delta":{"tool_calls":[{"index":0,"id":"a","function":{"name":"get_projects"}},{"index":1,"id":"b","function":{"name":"get_skills","arguments":"{}"}}]}}]}""")
                  + "data: [DONE]\n\n";
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));

        var events = await ReadAllAsync(_provider.StreamCompletionAsync(CompletionWithTool()));

        Assert.Equal(["get_projects", "get_skills"], events.Select(e => e.ToolCall!.Name).ToArray());
        Assert.Equal("{}", events[0].ToolCall!.Arguments);
    }

    [Fact]
    public async Task Stream_keeps_the_text_said_before_a_tool_call()
    {
        var sse = Chunk("""{"choices":[{"delta":{"content":"Let me check."}}]}""")
                  + Chunk("""{"choices":[{"delta":{"tool_calls":[{"index":0,"id":"a","function":{"name":"get_projects","arguments":"{}"}}]}}]}""")
                  + "data: [DONE]\n\n";
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, sse, "text/event-stream"));

        var events = await ReadAllAsync(_provider.StreamCompletionAsync(CompletionWithTool()));

        Assert.Equal("Let me check.", events[0].Token);
        Assert.True(events[1].IsToolCall);
    }

    [Fact]
    public async Task Tool_calls_and_results_are_sent_back_in_the_format_of_the_provider()
    {
        _api.StreamChatAsync(Arg.Any<ChatRequest>()).Returns(Response(HttpStatusCode.OK, "data: [DONE]\n\n", "text/event-stream"));
        var completion = new ProviderCompletionDto
        {
            Messages =
            [
                new ProviderMessageDto { Role = "User", Content = "project 5?" },
                new ProviderMessageDto
                {
                    Role = "Assistant",
                    Content = string.Empty,
                    ToolCalls = [new ProviderToolCallDto { Id = "call_1", Name = "get_project", Arguments = "{\"id\":5}" }],
                },
                new ProviderMessageDto { Role = "Tool", Content = "{\"name\":\"X\"}", ToolCallId = "call_1" },
            ],
        };

        await ReadAllAsync(_provider.StreamCompletionAsync(completion));

        var request = (ChatRequest)_api.ReceivedCalls().Single().GetArguments()[0]!;
        var assistant = request.Messages[1];
        var tool = request.Messages[2];

        Assert.Equal("assistant", assistant.Role);
        Assert.Null(assistant.Content);
        Assert.Equal("call_1", assistant.ToolCalls![0].Id);
        Assert.Equal("function", assistant.ToolCalls[0].Type);
        Assert.Equal("get_project", assistant.ToolCalls[0].Function!.Name);
        Assert.Equal("{\"id\":5}", assistant.ToolCalls[0].Function!.Arguments);
        Assert.Equal("tool", tool.Role);
        Assert.Equal("call_1", tool.ToolCallId);
    }
}
