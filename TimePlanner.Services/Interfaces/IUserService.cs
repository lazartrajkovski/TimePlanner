using TimePlanner.Domain.Entities;

namespace TimePlanner.Services.Interfaces
{
    public interface IUserService : IService<User>
    {
        Task<User?> GetByEmail(string email);
    }
}
