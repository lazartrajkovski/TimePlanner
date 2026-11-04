using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Repositories;

namespace TimePlanner.Repositories.Interfaces;

public interface ITimeEntryRepository : IBaseRepository<TimeEntry>
{
    Task<TimeEntry?> GetRunningByUserId(int userId);
    Task<IEnumerable<TimeEntry>> GetByAssignmentId(int assignmentId);
}