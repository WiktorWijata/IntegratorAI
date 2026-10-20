using System.Text.Json;
using IntegratorAI.Api.Middleware;
using IntegratorAI.BuildingBlocks.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Api.UnitTests;

public class GlobalExceptionHandlerTests
{
    private static async Task<(int Status, JsonElement Body)> Handle(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance)
            .TryHandleAsync(context, exception, CancellationToken.None);
        Assert.True(handled);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);

        return (context.Response.StatusCode, document.RootElement.Clone());
    }

    [Fact]
    public async Task A_provider_failure_is_a_502_that_does_not_repeat_what_the_provider_said()
    {
        var (status, body) = await Handle(new ProviderException("HuggingFace answered 402: You have no remaining credits.", 402));

        Assert.Equal(502, status);
        Assert.Equal("Bad Gateway", body.GetProperty("title").GetString());
        Assert.DoesNotContain("credits", body.GetProperty("detail").GetString());
        Assert.DoesNotContain("402", body.GetRawText());
    }

    [Fact]
    public async Task A_missing_entity_is_a_404_with_its_message()
    {
        var (status, body) = await Handle(new NotFoundException("Context", Guid.Empty));

        Assert.Equal(404, status);
        Assert.Contains("Context", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Anything_else_is_a_500()
    {
        var (status, _) = await Handle(new InvalidOperationException("boom"));

        Assert.Equal(500, status);
    }
}
