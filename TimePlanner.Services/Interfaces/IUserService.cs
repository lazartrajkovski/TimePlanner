using TimePlanner.Domain.Entities;

namespace TimePlanner.Services.Interfaces
{
    public interface IUserService : IBaseService<User>
    {
        Task<User?> GetByEmail(string email);
    }
}
