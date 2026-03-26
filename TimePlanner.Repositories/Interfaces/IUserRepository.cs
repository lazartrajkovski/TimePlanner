using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Repositories;

namespace TimePlanner.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmail(string email);
    }
}