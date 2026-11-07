using MapsterMapper;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.Services.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public TimeEntryService(ITimeEntryRepository timeEntryRepository, IAssignmentRepository assignmentRepository, ICurrentUser currentUser, IMapper mapper)
    {
        _timeEntryRepository = timeEntryRepository;
        _assignmentRepository = assignmentRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<TimeEntryResponse> Start(StartTimeEntryRequest request)
    {
        var assignment = await _assignmentRepository.GetByIdWithCategory(request.AssignmentId);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        var runningTimeEntry = await _timeEntryRepository.GetRunningByUserId(_currentUser.UserId);

        if (runningTimeEntry is not null)
        {
            throw new InvalidOperationException("A time entry is already running");
        }

        var timeEntry = new TimeEntry
        {
            AssignmentId = request.AssignmentId,
            UserId = _currentUser.UserId,
            StartedAt = DateTime.UtcNow,
        };

        var createdTimeEntry = await _timeEntryRepository.Add(timeEntry);

        return _mapper.Map<TimeEntryResponse>(createdTimeEntry);
    }

    public async Task<TimeEntryResponse> Stop()
    {
        var timeEntry = await _timeEntryRepository.GetRunningByUserId(_currentUser.UserId);

        if (timeEntry is null)
        {
            throw new KeyNotFoundException("No time entry is currently running");
        }

        timeEntry.EndedAt = DateTime.UtcNow;

        timeEntry = await _timeEntryRepository.Update(timeEntry);

        return _mapper.Map<TimeEntryResponse>(timeEntry);
    }

    public async Task<TimeEntryResponse?> GetActiveTimeEntry()
    {
        var timeEntry = await _timeEntryRepository.GetRunningByUserId(_currentUser.UserId);

        if (timeEntry is null)
        {
            return null;
        }

        return _mapper.Map<TimeEntryResponse>(timeEntry);
    }

    public async Task<IEnumerable<TimeEntryResponse>> GetByAssignmentId(int assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdWithCategory(assignmentId);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        var timeEntries = await _timeEntryRepository.GetByAssignmentId(assignmentId);

        return _mapper.Map<IEnumerable<TimeEntryResponse>>(timeEntries);
    }
}