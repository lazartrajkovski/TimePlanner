namespace TimePlanner.Services.DTOs
{
    public class UserLoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
