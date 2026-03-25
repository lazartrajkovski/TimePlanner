using TimePlanner.Domain.Entities;

namespace TimePlanner.Services.Interfaces
{
    public interface ICategoryService : IService<Category>
    {
        Task<IEnumerable<Category>> GetByUserId(int userId);
    }
}
