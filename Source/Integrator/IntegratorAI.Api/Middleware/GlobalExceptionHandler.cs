using IntegratorAI.BuildingBlocks.Application;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),

            // The message of a provider failure can describe the state of our account at the provider, so it only goes to the log.
            ProviderException => (StatusCodes.Status502BadGateway, "Bad Gateway", "The AI provider could not answer. Try again later."),

            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", exception.Message)
        };

        _logger.LogError(exception, "Unhandled exception: {Title} - {Message}", title, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
