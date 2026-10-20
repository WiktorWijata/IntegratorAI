using System.Runtime.CompilerServices;
using System.Text;
using IntegratorAI.Api.Results;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Chat.Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IntegratorAI.Api.UnitTests;

public class SseResultTests
{
    private static async IAsyncEnumerable<CompletionStreamEvent> Events(params CompletionStreamEvent[] events)
    {
        foreach (var item in events)
        {
            yield return item;
        }

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<CompletionStreamEvent> Failing([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        throw new ProviderException("the provider is down", 503);

#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }

    private static async Task<(DefaultHttpContext Context, string Body)> Run(IAsyncEnumerable<CompletionStreamEvent> stream)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await new SseResult(stream).ExecuteResultAsync(new ActionContext(context, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));

        return (context, Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray()));
    }

    [Theory]
    [InlineData("hello", "data: hello\n\n")]
    [InlineData("", "data: \n\n")]
    [InlineData("a\nb", "data: a\ndata: b\n\n")]
    [InlineData("a\r\nb", "data: a\ndata: b\n\n")]
    [InlineData("list:\n- one\n- two\n", "data: list:\ndata: - one\ndata: - two\ndata: \n\n")]
    [InlineData("\n", "data: \ndata: \n\n")]
    public void A_line_break_in_a_token_never_ends_the_event(string token, string expectedFrame)
    {
        Assert.Equal(expectedFrame, SseResult.Frame(token));
    }

    [Fact]
    public async Task Tokens_are_streamed_as_events_followed_by_done_and_the_id_goes_to_a_header()
    {
        var completionId = Guid.NewGuid();

        var (context, body) = await Run(Events(
            CompletionStreamEvent.ForCompletion(completionId),
            CompletionStreamEvent.ForToken("Hi"),
            CompletionStreamEvent.ForToken("there\n- x")));

        Assert.Equal("data: Hi\n\ndata: there\ndata: - x\n\ndata: [DONE]\n\n", body);
        Assert.Equal(completionId.ToString(), context.Response.Headers["Completion-Id"].ToString());
        Assert.Equal("text/event-stream", context.Response.ContentType);
    }

    [Fact]
    public async Task A_failure_before_the_first_token_writes_nothing_so_the_error_handler_can_still_answer()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await Assert.ThrowsAsync<ProviderException>(() =>
            new SseResult(Failing()).ExecuteResultAsync(new ActionContext(context, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor())));

        Assert.Equal(0, context.Response.Body.Length);
        Assert.False(context.Response.HasStarted);
    }
}
