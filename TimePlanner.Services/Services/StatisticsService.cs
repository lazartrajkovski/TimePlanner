using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

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
        var user = await _userRepository.GetById(_currentUser.UserId);

        if (user is null)
        {
            throw new ArgumentException("User not found");
        }
        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
        
        var utcNow = DateTime.UtcNow;
        
        var userLocaTimelNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTimeZone);

        var localDayStart = userLocaTimelNow.Date;
        var localDayEnd = localDayStart.AddDays(1);
        
        var utcDayStart = TimeZoneInfo.ConvertTimeToUtc(localDayStart, userTimeZone);
        var utcDayEnd = TimeZoneInfo.ConvertTimeToUtc(localDayEnd, userTimeZone);

        var timeEntries = await _timeEntryRepository.GetByUserIdAndDateRange(_currentUser.UserId,
            utcDayStart, utcDayEnd);

        long totalTrackedSeconds = 0;
        
        foreach (var timeEntry in timeEntries)
        {
            var countedStart = timeEntry.StartedAt < utcDayStart
                ? utcDayStart
                : timeEntry.StartedAt;

            var entryEnd = timeEntry.EndedAt ?? utcNow;

            var countedEnd = entryEnd > utcDayEnd
                ? utcDayEnd
                : entryEnd;

            if (countedEnd <= countedStart)
            {
                continue;
            }
            
            totalTrackedSeconds += (long)(countedEnd - countedStart).TotalSeconds;
        }
        
        return new StatisticsResponse
        {
            TotalTrackedTime = totalTrackedSeconds
        };
    }
}