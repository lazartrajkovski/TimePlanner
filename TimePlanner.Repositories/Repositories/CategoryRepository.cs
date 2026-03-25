using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Data;
using TimePlanner.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TimePlanner.Repositories.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(TimePlannerDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Category>> GetByUserId(int userId)
        {
            return await _context.Set<Category>()
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
    }
}
