using TimePlanner.Services.Interfaces;

namespace TimePlanner.API.Identity;

public class DevelopmentCurrentUser : ICurrentUser
{
    public int UserId => 1;
}