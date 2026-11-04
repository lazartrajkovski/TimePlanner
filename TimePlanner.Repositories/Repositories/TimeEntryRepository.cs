using Microsoft.EntityFrameworkCore;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Data;
using TimePlanner.Repositories.Interfaces;

namespace TimePlanner.Repositories.Repositories;

public class TimeEntryRepository : BaseRepository<TimeEntry>, ITimeEntryRepository
{
    public TimeEntryRepository(TimePlannerDbContext context) : base(context)
    {
    }
    
    public async Task<TimeEntry?> GetRunningByUserId(int userId)
    {
        return await _context.Set<TimeEntry>()
            .Where(e => e.UserId == userId && e.EndedAt == null)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetByAssignmentId(int assignmentId)
    {
        return await _context.Set<TimeEntry>()
            .Where(e => e.AssignmentId == assignmentId)
            .ToListAsync();
    }
}