using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using TimePlanner.API.Identity;
using TimePlanner.Services.Exceptions;

namespace TimePlanner.Tests.API.Identity;

public class HttpCurrentUserTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly HttpCurrentUser _currentUser;

    public HttpCurrentUserTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _currentUser = new HttpCurrentUser(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void UserId_ValidUserIdClaim_ReturnsUserId()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "42")],
                "TestAuthentication")),
        };

        _httpContextAccessorMock
            .Setup(accessor => accessor.HttpContext)
            .Returns(httpContext);

        var result = _currentUser.UserId;

        Assert.Equal(42, result);
    }

    [Fact]
    public void UserId_NoUserIdClaim_ThrowsUnauthorizedException()
    {
        var httpContext = new DefaultHttpContext();

        _httpContextAccessorMock
            .Setup(accessor => accessor.HttpContext)
            .Returns(httpContext);

        var exception = Assert.Throws<UnauthorizedException>(
            () => _currentUser.UserId);

        Assert.Equal("No authenticated user", exception.Message);
    }

    [Fact]
    public void UserId_NonNumericUserIdClaim_ThrowsUnauthorizedException()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "testuser")],
                "TestAuthentication")),
        };

        _httpContextAccessorMock
            .Setup(accessor => accessor.HttpContext)
            .Returns(httpContext);

        var exception = Assert.Throws<UnauthorizedException>(
            () => _currentUser.UserId);

        Assert.Equal("No authenticated user", exception.Message);
    }

    [Fact]
    public void UserId_NoHttpContext_ThrowsUnauthorizedException()
    {
        _httpContextAccessorMock
            .Setup(accessor => accessor.HttpContext)
            .Returns((HttpContext?)null);

        var exception = Assert.Throws<UnauthorizedException>(
            () => _currentUser.UserId);

        Assert.Equal("No authenticated user", exception.Message);
    }
}