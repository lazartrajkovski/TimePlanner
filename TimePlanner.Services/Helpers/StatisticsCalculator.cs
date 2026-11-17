using TimePlanner.Domain.Entities;
using TimePlanner.Services.DTOs.Statistics;

namespace TimePlanner.Services.Helpers;

public static class StatisticsCalculator
{
    public static long GetTrackedSeconds(TimeEntry timeEntry, StatisticsRange range, DateTime utcNow)
    {
        var countedStart = timeEntry.StartedAt < range.StartUtc
            ? range.StartUtc
            : timeEntry.StartedAt;

        var entryEnd = timeEntry.EndedAt ?? utcNow;

        var countedEnd = entryEnd > range.EndUtc
            ? range.EndUtc
            : entryEnd;

        if (countedEnd <= countedStart)
        {
            return 0;
        }

        return (long)(countedEnd - countedStart).TotalSeconds;
    }

    public static List<TrackedTimeIntervalResponse> CalculateHourlyTrackedTime(IReadOnlyList<TimeEntry> timeEntries, StatisticsRange dayRange, TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var trackedSecondsByLocalHour = new long[24];

        // Iterate through the real UTC hours because a local day can have 23, 24 or 25 hours.
        for (var hourStartUtc = dayRange.StartUtc; hourStartUtc < dayRange.EndUtc; hourStartUtc = hourStartUtc.AddHours(1))
        {
            var hourEndUtc = hourStartUtc.AddHours(1);

            if (hourEndUtc > dayRange.EndUtc)
            {
                hourEndUtc = dayRange.EndUtc;
            }

            var hourRange = new StatisticsRange
            {
                StartUtc = hourStartUtc,
                EndUtc = hourEndUtc,
            };

            var localHour = TimeZoneInfo.ConvertTimeFromUtc(hourStartUtc, userTimeZone).Hour;

            foreach (var timeEntry in timeEntries)
            {
                trackedSecondsByLocalHour[localHour] += GetTrackedSeconds(timeEntry, hourRange, utcNow);
            }
        }

        var hourlyTrackedTime = new List<TrackedTimeIntervalResponse>();

        for (var hour = 0; hour < 24; hour++)
        {
            if (trackedSecondsByLocalHour[hour] == 0)
            {
                continue;
            }

            hourlyTrackedTime.Add(new TrackedTimeIntervalResponse
            {
                StartDate = dayRange.StartDate,
                EndDate = dayRange.EndDate,
                Hour = hour,
                TrackedSeconds = trackedSecondsByLocalHour[hour],
            });
        }

        return hourlyTrackedTime;
    }

    public static List<TrackedTimeIntervalResponse> CalculateDailyTrackedTime(IReadOnlyList<TimeEntry> timeEntries,
        StatisticsRange range, TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var dailyTrackedTime = new List<TrackedTimeIntervalResponse>();

        for (var currentDay = range.StartDate; currentDay <= range.EndDate; currentDay = currentDay.AddDays(1))
        {
            // A one-day range has the same inclusive start and end date.
            var currentDayRange = StatisticsRangeResolver.ConvertToUtcRange(currentDay, currentDay, userTimeZone);

            long dailyTrackedSeconds = 0;

            foreach (var timeEntry in timeEntries)
            {
                dailyTrackedSeconds += GetTrackedSeconds(
                    timeEntry,
                    currentDayRange,
                    utcNow);
            }

            dailyTrackedTime.Add(new TrackedTimeIntervalResponse
            {
                StartDate = currentDay,
                EndDate = currentDay,
                TrackedSeconds = dailyTrackedSeconds,
            });
        }

        return dailyTrackedTime;
    }
}