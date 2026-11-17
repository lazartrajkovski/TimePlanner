namespace TimePlanner.Services.DTOs.Users
{
    public class UserLoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}