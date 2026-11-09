using MapsterMapper;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
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
        if (request.AssignmentId <= 0)
        {
            throw new ArgumentException("AssignmentId must be greater than zero");
        }

        var assignment = await _assignmentRepository.GetByIdWithCategory(request.AssignmentId);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        if (assignment.Status == AssignmentStatus.Done)
        {
            throw new InvalidOperationException("Assignment is already done");
        }

        var runningTimeEntry = await _timeEntryRepository.GetRunningByUserId(_currentUser.UserId);

        if (runningTimeEntry is not null)
        {
            throw new InvalidOperationException("A time entry is already running");
        }

        if (assignment.Status == AssignmentStatus.Pending)
        {
            assignment.Status = AssignmentStatus.InProgress;
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
            throw new InvalidOperationException("No time entry is currently running");
        }

        timeEntry.EndedAt = DateTime.UtcNow;

        timeEntry = await _timeEntryRepository.Update(timeEntry);

        return _mapper.Map<TimeEntryResponse>(timeEntry);
    }

    public async Task<TimeEntryResponse?> GetActiveTimeEntry()
    {
        var timeEntry = await _timeEntryRepository.GetRunningByUserId(_currentUser.UserId);

        return timeEntry is null ? null : _mapper.Map<TimeEntryResponse>(timeEntry);
    }

    public async Task<IEnumerable<TimeEntryResponse>> GetByAssignmentId(int assignmentId)
    {
        if (assignmentId <= 0)
        {
            throw new ArgumentException("AssignmentId must be greater than zero");
        }

        var assignment = await _assignmentRepository.GetByIdWithCategory(assignmentId);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        var timeEntries = await _timeEntryRepository.GetByAssignmentId(assignmentId);

        return _mapper.Map<IEnumerable<TimeEntryResponse>>(timeEntries);
    }
}