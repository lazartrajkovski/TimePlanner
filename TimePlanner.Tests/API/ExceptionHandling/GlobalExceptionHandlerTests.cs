using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TimePlanner.API.ExceptionHandling;
using TimePlanner.Services.Exceptions;

namespace TimePlanner.Tests.API.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
    }

    public static TheoryData<Exception, int, string> ApplicationExceptions => new()
    {
        { new ValidationException("Validation failed"), StatusCodes.Status400BadRequest, "Bad Request" },
        { new UnauthorizedException("Not authenticated"), StatusCodes.Status401Unauthorized, "Unauthorized" },
        { new NotFoundException("Resource not found"), StatusCodes.Status404NotFound, "Not Found" },
        { new ConflictException("Resource conflict"), StatusCodes.Status409Conflict, "Conflict" }
    };

    [Theory]
    [MemberData(nameof(ApplicationExceptions))]
    public async Task TryHandleAsync_ApplicationException_WritesMappedProblemDetails(
        Exception exception, int expectedStatusCode, string expectedTitle)
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "test-trace-id"
        };
        httpContext.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(httpContext.Response.Body);
        var body = json.RootElement;

        Assert.Equal(expectedStatusCode, body.GetProperty("status").GetInt32());
        Assert.Equal(expectedTitle, body.GetProperty("title").GetString());
        Assert.Equal(exception.Message, body.GetProperty("detail").GetString());
        Assert.Equal("test-trace-id", body.GetProperty("traceId").GetString());
    }
}