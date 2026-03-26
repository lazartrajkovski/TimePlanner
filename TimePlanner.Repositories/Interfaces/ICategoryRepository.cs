using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Repositories;

namespace TimePlanner.Repositories.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<Category>> GetByUserId(int userId);
    }
}