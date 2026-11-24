using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TimePlanner.API.Identity;
using TimePlanner.Domain.Entities;

namespace TimePlanner.Tests.API.Identity;

public class JwtTokenServiceTests
{
    private readonly JwtSettings _settings;
    private readonly JwtTokenService _tokenService;

    public JwtTokenServiceTests()
    {
        _settings = new JwtSettings
        {
            Key = "test-signing-key-that-is-at-least-32-characters-long",
            Issuer = "TimePlanner.Tests",
            Audience = "TimePlanner.Tests.Client",
            ExpirationMinutes = 60,
        };

        _tokenService = new JwtTokenService(Options.Create(_settings));
    }

    [Fact]
    public void GenerateToken_User_ReturnsSignedTokenWithUserClaims()
    {
        var user = new User
        {
            Id = 42,
            Username = "testuser",
        };

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,

            ValidateAudience = true,
            ValidAudience = _settings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.Key)),

            ValidateLifetime = true,
        };

        var before = DateTime.UtcNow;

        var token = _tokenService.GenerateToken(user);

        var after = DateTime.UtcNow;

        var principal = new JwtSecurityTokenHandler().ValidateToken(
            token,
            validationParameters,
            out var validatedToken);

        Assert.Equal("42", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("testuser", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.InRange(validatedToken.ValidTo, before.AddMinutes(60).AddSeconds(-1), after.AddMinutes(60));
    }
}