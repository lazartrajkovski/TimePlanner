using TimePlanner.Domain.Entities;

namespace TimePlanner.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}