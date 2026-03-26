using Microsoft.EntityFrameworkCore;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Data;
using TimePlanner.Repositories.Interfaces;

namespace TimePlanner.Repositories.Repositories
{
    public class AssignmentRepository : BaseRepository<Assignment>, IAssignmentRepository
    {
        public AssignmentRepository(TimePlannerDbContext context) : base(context)
        {
            
        }

        public async Task<IEnumerable<Assignment>> GetByUserId(int userId)
        {
            return await _context.Set<Assignment>()
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetByCategoryId(int categoryId)
        {
            return await _context.Set<Assignment>()
                .Where(a => a.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetByStatus(int userId, AssignmentStatus status)
        {
            return await _context.Set<Assignment>()
                .Where(a => a.UserId == userId && a.Status == status)
                .ToListAsync();
        }
    }
}
