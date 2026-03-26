using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.Services.Services
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository) : base(categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetByUserId(int userId)
        {
            return await _categoryRepository.GetByUserId(userId);
        }
    }
}
