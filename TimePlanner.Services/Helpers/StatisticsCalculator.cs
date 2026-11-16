using TimePlanner.Domain.Entities;

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
}