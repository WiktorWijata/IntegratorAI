using System.Text;
using System.Text.Json;
using IntegratorAI.Api.Middleware;
using IntegratorAI.Api.Results;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Chat.Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegratorAI.Api.UnitTests;

public class ToolCallStreamTests
{
    private static async IAsyncEnumerable<CompletionStreamEvent> Events(params CompletionStreamEvent[] events)
    {
        foreach (var item in events)
            yield return item;

        await Task.CompletedTask;
    }

    private static async Task<string> RunAsync(IAsyncEnumerable<CompletionStreamEvent> stream)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await new SseResult(stream).ExecuteResultAsync(new ActionContext(context, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));

        return Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray());
    }

    [Fact]
    public void A_tool_call_is_an_event_with_the_arguments_as_a_json_object()
    {
        var frame = SseResult.ToolCallFrame(new ToolCallDto { Id = "call_1", Name = "get_project", Arguments = "{\"id\":5}" });

        Assert.StartsWith("event: tool_call\ndata: ", frame);
        Assert.EndsWith("\n\n", frame);
        var data = frame["event: tool_call\ndata: ".Length..].TrimEnd('\n');
        using var document = JsonDocument.Parse(data);
        Assert.Equal("call_1", document.RootElement.GetProperty("id").GetString());
        Assert.Equal("get_project", document.RootElement.GetProperty("name").GetString());
        Assert.Equal(5, document.RootElement.GetProperty("arguments").GetProperty("id").GetInt32());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not json")]
    public void Missing_or_broken_arguments_become_an_empty_object(string? arguments)
    {
        var frame = SseResult.ToolCallFrame(new ToolCallDto { Id = "c", Name = "n", Arguments = arguments! });

        var data = frame["event: tool_call\ndata: ".Length..].TrimEnd('\n');
        using var document = JsonDocument.Parse(data);
        Assert.Equal(JsonValueKind.Object, document.RootElement.GetProperty("arguments").ValueKind);
        Assert.Empty(document.RootElement.GetProperty("arguments").EnumerateObject());
    }

    [Fact]
    public async Task The_stream_carries_tokens_and_tool_calls_and_ends_with_done()
    {
        var body = await RunAsync(Events(
            CompletionStreamEvent.ForCompletion(Guid.NewGuid()),
            CompletionStreamEvent.ForToken("Let me check."),
            CompletionStreamEvent.ForToolCall(new ToolCallDto { Id = "call_1", Name = "get_projects", Arguments = "{}" })));

        Assert.StartsWith("data: Let me check.\n\nevent: tool_call\ndata: {", body);
        Assert.EndsWith("data: [DONE]\n\n", body);
    }

    [Fact]
    public async Task A_conflict_is_a_409_with_its_message()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance)
            .TryHandleAsync(context, new ConflictException("The completion is waiting for tool results."), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(409, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("Conflict", document.RootElement.GetProperty("title").GetString());
        Assert.Equal("The completion is waiting for tool results.", document.RootElement.GetProperty("detail").GetString());
    }
}
