using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Repositories;

namespace TimePlanner.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmail(string email);
    }
}