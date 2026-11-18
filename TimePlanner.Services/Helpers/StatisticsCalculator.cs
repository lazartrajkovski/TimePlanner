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

    public static List<TrackedTimeIntervalResponse> CalculateWeeklyTrackedTime(IReadOnlyList<TimeEntry> timeEntries,
        StatisticsRange range, TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var weeklyTrackedTime = new List<TrackedTimeIntervalResponse>();
        var weekStartDate = range.StartDate;

        while (weekStartDate <= range.EndDate)
        {
            var weekEndDate = weekStartDate;

            // Move to Sunday or the end of the requested range, whichever comes first.
            while (weekEndDate.DayOfWeek != DayOfWeek.Sunday && weekEndDate < range.EndDate)
            {
                weekEndDate = weekEndDate.AddDays(1);
            }

            var weekRange = StatisticsRangeResolver.ConvertToUtcRange(weekStartDate, weekEndDate, userTimeZone);

            long weeklyTrackedSeconds = 0;

            foreach (var timeEntry in timeEntries)
            {
                weeklyTrackedSeconds += GetTrackedSeconds(timeEntry, weekRange, utcNow);
            }

            weeklyTrackedTime.Add(new TrackedTimeIntervalResponse
            {
                StartDate = weekStartDate,
                EndDate = weekEndDate,
                TrackedSeconds = weeklyTrackedSeconds,
            });

            weekStartDate = weekEndDate.AddDays(1);
        }

        return weeklyTrackedTime;
    }

    public static List<TrackedTimeIntervalResponse> CalculateMonthlyTrackedTime(IReadOnlyList<TimeEntry> timeEntries,
        StatisticsRange range, TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var monthlyTrackedTime = new List<TrackedTimeIntervalResponse>();
        var monthStartDate = range.StartDate;

        while (monthStartDate <= range.EndDate)
        {
            var daysInMonth = DateTime.DaysInMonth(monthStartDate.Year, monthStartDate.Month);
            var monthEndDate = new DateOnly(monthStartDate.Year, monthStartDate.Month, daysInMonth);

            if (monthEndDate > range.EndDate)
            {
                monthEndDate = range.EndDate;
            }

            var monthRange = StatisticsRangeResolver.ConvertToUtcRange(monthStartDate, monthEndDate, userTimeZone);

            long monthlyTrackedSeconds = 0;

            foreach (var timeEntry in timeEntries)
            {
                monthlyTrackedSeconds += GetTrackedSeconds(timeEntry, monthRange, utcNow);
            }

            monthlyTrackedTime.Add(new TrackedTimeIntervalResponse
            {
                StartDate = monthStartDate,
                EndDate = monthEndDate,
                TrackedSeconds = monthlyTrackedSeconds,
            });

            monthStartDate = monthEndDate.AddDays(1);
        }

        return monthlyTrackedTime;
    }
}