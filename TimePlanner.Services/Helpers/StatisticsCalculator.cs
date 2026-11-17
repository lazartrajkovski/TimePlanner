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

    public static List<TrackedTimeIntervalResponse> CalculateHourlyTrackedTime(IReadOnlyList<TimeEntry> timeEntries, StatisticsRange dayRange, DateTime utcNow)
    {
        var hourlyTrackedTime = new List<TrackedTimeIntervalResponse>();

        for (var hour = 0; hour < 24; hour++)
        {
            var startUtc = dayRange.StartUtc.AddHours(hour);
            var endUtc = dayRange.StartUtc.AddHours(hour + 1);

            var hourRange = new StatisticsRange()
            {
                StartUtc = startUtc,
                EndUtc = endUtc,
            };

            long trackedSeconds = 0;

            foreach (var timeEntry in timeEntries)
            {
                trackedSeconds += GetTrackedSeconds(timeEntry, hourRange, utcNow);
            }

            if (trackedSeconds > 0)
            {
                hourlyTrackedTime.Add(new TrackedTimeIntervalResponse
                {
                    Hour = hour,
                    TrackedSeconds = trackedSeconds,
                    StartDate = dayRange.StartDate,
                    EndDate = dayRange.EndDate,
                });
            }
        }

        return hourlyTrackedTime;
    }
}