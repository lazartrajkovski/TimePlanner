using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TimePlanner.API.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. RequestId: {RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal Server Error"
        };

        var problemDetails = new ProblemDetails()
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error has occurred"
                : exception.Message
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            problemDetails.Extensions["requestId"] = httpContext.TraceIdentifier;
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}