using System.Text.Json;
using System.Text.Json.Nodes;
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

            var frame = item.Type == CompletionStreamEventType.ToolCall ? ToolCallFrame(item.ToolCall) : Frame(item.Data);
            await response.WriteAsync(frame, cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }

        await response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// A request of the model to run a tool, as the event <c>tool_call</c>. The arguments are written as a JSON object.
    /// </summary>
    public static string ToolCallFrame(ToolCallDto toolCall)
    {
        JsonNode? arguments = null;
        try
        {
            arguments = JsonNode.Parse(string.IsNullOrWhiteSpace(toolCall.Arguments) ? "{}" : toolCall.Arguments);
        }
        catch (JsonException)
        {
            // The model wrote arguments that are not JSON; the client will find that they do not fit the tool.
        }

        var json = new JsonObject
        {
            ["id"] = toolCall.Id,
            ["name"] = toolCall.Name,
            ["arguments"] = arguments ?? new JsonObject(),
        }.ToJsonString();

        return $"event: tool_call\ndata: {json}\n\n";
    }

    /// <summary>
    /// One Server-Sent Event. A token that contains a line break has to be split into several <c>data:</c> lines:
    /// a client joins them back with "\n", whereas a bare line break would end the event and drop the rest of the token.
    /// </summary>
    public static string Frame(string data)
    {
        var lines = data.Replace("\r\n", "\n").Split('\n');

        return string.Concat(lines.Select(line => $"data: {line}\n")) + "\n";
    }
}
