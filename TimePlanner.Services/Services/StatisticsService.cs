using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs.Statistics;
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
        var interval = StatisticsIntervalResolver.Resolve(request, range);

        var timeEntries = await _timeEntryRepository.GetByUserIdAndDateRange(_currentUser.UserId,
           range.StartUtc, range.EndUtc);

        var trackedTime = interval switch
        {
            StatisticsInterval.Hour =>
                StatisticsCalculator.CalculateHourlyTrackedTime(
                    timeEntries,
                    range,
                    userTimeZone,
                    utcNow),

            StatisticsInterval.Day =>
                StatisticsCalculator.CalculateDailyTrackedTime(
                    timeEntries,
                    range,
                    userTimeZone,
                    utcNow),

            StatisticsInterval.Week =>
                StatisticsCalculator.CalculateWeeklyTrackedTime(
                    timeEntries,
                    range,
                    userTimeZone,
                    utcNow),

            _ => throw new ArgumentException("Invalid statistics interval")
        };

        long totalTrackedSeconds = 0;

        foreach (var trackedTimeInterval in trackedTime)
        {
            totalTrackedSeconds += trackedTimeInterval.TrackedSeconds;
        }

        return new StatisticsResponse
        {
            Period = request.Period,
            RangeStartDate = range.StartDate,
            RangeEndDate = range.EndDate,
            TotalTrackedSeconds = totalTrackedSeconds,
            TrackedTime = trackedTime,
            Interval = interval,
        };
    }
}