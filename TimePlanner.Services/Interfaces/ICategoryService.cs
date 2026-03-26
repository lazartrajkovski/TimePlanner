using TimePlanner.Domain.Entities;

namespace TimePlanner.Services.Interfaces
{
    public interface ICategoryService : IBaseService<Category>
    {
        Task<IEnumerable<Category>> GetByUserId(int userId);
    }
}
