using System.Net.Mail;
using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Validators;

public static class UserValidator
{
    public static void ValidateAndNormalize(UserRegisterRequest request)
    {
        request.Username = NormalizeUsername(request.Username);
        request.Email = NormalizeEmail(request.Email);
        ValidatePassword(request.Password);
        request.FirstName = TextInput.Required(request.FirstName, "First name", 50);
        request.LastName = TextInput.Required(request.LastName, "Last name", 50);
        request.TimeZoneId = NormalizeTimeZoneId(request.TimeZoneId);
    }

    public static void ValidateAndNormalize(UserLoginRequest request)
    {
        request.Username = NormalizeUsername(request.Username);
    }

    private static string NormalizeUsername(string username)
    {
        return TextInput.Required(username, "Username", 50).ToLowerInvariant();
    }

    private static string NormalizeEmail(string email)
    {
        var normalizedEmail = TextInput.Required(email, "Email", 100).ToLowerInvariant();

        if (!MailAddress.TryCreate(normalizedEmail, out var parsedEmail) ||
            parsedEmail.Address != normalizedEmail)
        {
            throw new ArgumentException("Invalid email address");
        }

        return normalizedEmail;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required");
        }

        if (password.Length < 15)
        {
            throw new ArgumentException("Password must be at least 15 characters");
        }

        if (password.Length > 128)
        {
            throw new ArgumentException("Password must be 128 characters or fewer");
        }
    }

    private static string NormalizeTimeZoneId(string timeZoneId)
    {
        var normalizedTimeZoneId = TextInput.Required(timeZoneId, "Time zone", 100);

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(normalizedTimeZoneId, out _))
        {
            throw new ArgumentException("Invalid time zone");
        }

        return normalizedTimeZoneId;
    }
}