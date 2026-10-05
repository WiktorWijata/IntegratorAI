using IntegratorAI.Chat.Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegratorAI.Api.Results;

public class SseResult : IActionResult
{
    public const string CompletionIdHeader = "Completion-Id";

    private readonly IAsyncEnumerable<CompletionStreamEvent> _stream;

    public SseResult(IAsyncEnumerable<string> stream)
        : this(stream.Select(CompletionStreamEvent.ForToken))
    {
    }

    public SseResult(IAsyncEnumerable<CompletionStreamEvent> stream)
    {
        _stream = stream;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        var cancellationToken = context.HttpContext.RequestAborted;

        response.ContentType = "text/event-stream";

        await foreach (var item in _stream.WithCancellation(cancellationToken))
        {
            if (item.Type == CompletionStreamEventType.Completion)
            {
                // Must be set before the first body write, which is guaranteed as the
                // completion event is always the first item of a stream.
                response.Headers[CompletionIdHeader] = item.Data;
                continue;
            }

            await response.WriteAsync($"data: {item.Data}\n\n", cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }

        await response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
