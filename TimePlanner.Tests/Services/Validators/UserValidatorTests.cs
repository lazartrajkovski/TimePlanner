using TimePlanner.Services.DTOs.Users;
using TimePlanner.Services.Validators;

namespace TimePlanner.Tests.Services.Validators;

public class UserValidatorTests
{
    [Fact]
    public void ValidateAndNormalize_RegisterPasswordShorterThanMinimum_ThrowsArgumentException()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = new string('a', 14),
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = Assert.Throws<ArgumentException>(() => UserValidator.ValidateAndNormalize(request));

        Assert.Equal("Password must be at least 15 characters", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterPasswordAtMinimumLength_DoesNotThrowException()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = new string('a', 15),
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = Record.Exception(() => UserValidator.ValidateAndNormalize(request));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterPasswordAtMaximumLength_DoesNotThrowException()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = new string('a', 128),
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = Record.Exception(() => UserValidator.ValidateAndNormalize(request));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterPasswordLongerThanMaximum_ThrowsArgumentException()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = new string('a', 129),
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = Assert.Throws<ArgumentException>(() => UserValidator.ValidateAndNormalize(request));

        Assert.Equal("Password must be 128 characters or fewer", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterPasswordWithSurroundingSpaces_KeepsPasswordUnchanged()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "  VerySecurePassword123!  ",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        UserValidator.ValidateAndNormalize(request);

        Assert.Equal(
            "  VerySecurePassword123!  ",
            request.Password);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalize_RegisterMissingPassword_ThrowsArgumentException(string? password)
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = password!,
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = Assert.Throws<ArgumentException>(() => UserValidator.ValidateAndNormalize(request));

        Assert.Equal("Password is required", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterEmailWithMixedCase_NormalizesEmail()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "TEST@EXAMPLE.COM ",
            Password = "VeryStrongPassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        UserValidator.ValidateAndNormalize(request);

        Assert.Equal("test@example.com", request.Email);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterInvalidEmail_ThrowsArgumentException()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "Not-an-Email",
            Password = "VeryStrongPassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = Assert.Throws<ArgumentException>(() => UserValidator.ValidateAndNormalize(request));

        Assert.Equal("Invalid email address", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterEmailWithDisplayName_ThrowsArgumentException()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "Test <test@example.com>",
            Password = "VerySecurePassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Europe/Skopje",
        };

        var exception = Assert.Throws<ArgumentException>(() => UserValidator.ValidateAndNormalize(request));

        Assert.Equal("Invalid email address", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterInvalidTimeZone_ThrowsArgumentException()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "VerySecurePassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = "Not/AZone",
        };

        var exception = Assert.Throws<ArgumentException>(() => UserValidator.ValidateAndNormalize(request));

        Assert.Equal("Invalid time zone", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalize_RegisterTimeZoneWithWhitespace_TrimsTimeZone()
    {
        var request = new UserRegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "VerySecurePassword123!",
            FirstName = "Test",
            LastName = "User",
            TimeZoneId = " Europe/Skopje ",
        };

        UserValidator.ValidateAndNormalize(request);

        Assert.Equal("Europe/Skopje", request.TimeZoneId);
    }

    [Fact]
    public void ValidateAndNormalize_LoginUsernameWithMixedCaseAndWhitespace_NormalizesUsername()
    {
        var request = new UserLoginRequest
        {
            Username = " TestUser ",
            Password = "password",
        };

        UserValidator.ValidateAndNormalize(request);

        Assert.Equal("testuser", request.Username);
    }

    [Fact]
    public void ValidateAndNormalize_LoginPasswordWithSurroundingSpaces_KeepsPasswordUnchanged()
    {
        var request = new UserLoginRequest
        {
            Username = "testuser",
            Password = "  VerySecurePassword123!  ",
        };

        UserValidator.ValidateAndNormalize(request);

        Assert.Equal(
            "  VerySecurePassword123!  ",
            request.Password);
    }
}