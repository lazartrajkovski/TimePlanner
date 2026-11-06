using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Interfaces;

public interface ITimeEntryService
{
    Task<TimeEntryResponse> Start(StartTimeEntryRequest request);
    Task<TimeEntryResponse> Stop();
    Task<TimeEntryResponse?> GetActiveTimeEntry();
    Task<IEnumerable<TimeEntryResponse>> GetByAssignmentId(int assignmentId);
}