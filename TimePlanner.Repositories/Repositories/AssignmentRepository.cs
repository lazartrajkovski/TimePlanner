using Microsoft.EntityFrameworkCore;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Data;
using TimePlanner.Repositories.Interfaces;

namespace TimePlanner.Repositories.Repositories;

public class AssignmentRepository : BaseRepository<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(TimePlannerDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Assignment>> GetByUserId(int userId, AssignmentStatus? status)
    {
        var query = _context.Set<Assignment>()
            .AsNoTracking()
            .Where(a => a.Category.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Assignment>> GetByCategoryId(int categoryId)
    {
        return await _context.Set<Assignment>()
            .AsNoTracking()
            .Where(a => a.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<Assignment?> GetByIdWithCategory(int assignmentId)
    {
        return await _context.Set<Assignment>()
            .Include(a => a.Category)
            .Where(a => a.Id == assignmentId)
            .FirstOrDefaultAsync();
    }
}