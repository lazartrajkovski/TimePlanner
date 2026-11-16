using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Helpers;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Validators;

namespace TimePlanner.Services.Services;

public class StatisticsService : IStatisticsService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public StatisticsService(ITimeEntryRepository timeEntryRepository, IUserRepository userRepository, ICurrentUser currentUser)
    {
        _timeEntryRepository = timeEntryRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<StatisticsResponse> GetStatistics(StatisticsRequest request)
    {
        StatisticsValidator.Validate(request);

        var user = await _userRepository.GetById(_currentUser.UserId);

        if (user is null)
        {
            throw new ArgumentException("User not found");
        }
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);

        var utcNow = DateTime.UtcNow;

        var range = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        var timeEntries = await _timeEntryRepository.GetByUserIdAndDateRange(_currentUser.UserId,
           range.StartUtc, range.EndUtc);

        long totalTrackedSeconds = 0;

        foreach (var timeEntry in timeEntries)
        {
            totalTrackedSeconds += GetTrackedSeconds(timeEntry, range.StartUtc, range.EndUtc, utcNow);
        }

        return new StatisticsResponse
        {
            TotalTrackedTime = totalTrackedSeconds
        };
    }

    private static long GetTrackedSeconds(TimeEntry timeEntry, DateTime rangeStartUtc, DateTime rangeEndUtc, DateTime utcNow)
    {
        var countedStart = timeEntry.StartedAt < rangeStartUtc
            ? rangeStartUtc
            : timeEntry.StartedAt;

        var entryEnd = timeEntry.EndedAt ?? utcNow;

        var countedEnd = entryEnd > rangeEndUtc
            ? rangeEndUtc
            : entryEnd;

        if (countedEnd <= countedStart)
        {
            return 0;
        }

        return (long)(countedEnd - countedStart).TotalSeconds;
    }
}