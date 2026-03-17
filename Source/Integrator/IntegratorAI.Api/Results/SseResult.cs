using Microsoft.AspNetCore.Mvc;

namespace IntegratorAI.Api.Results;

public class SseResult : IActionResult
{
    private readonly IAsyncEnumerable<string> _stream;

    public SseResult(IAsyncEnumerable<string> stream)
    {
        _stream = stream;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        var cancellationToken = context.HttpContext.RequestAborted;

        response.ContentType = "text/event-stream";

        await foreach (var token in _stream.WithCancellation(cancellationToken))
        {
            await response.WriteAsync($"data: {token}\n\n", cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }

        await response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
