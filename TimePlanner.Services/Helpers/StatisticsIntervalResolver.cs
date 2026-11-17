using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs.Statistics;

namespace TimePlanner.Services.Helpers;

public static class StatisticsIntervalResolver
{
    public static StatisticsInterval Resolve(StatisticsRequest request, StatisticsRange range)
    {
        return request.Period switch
        {
            StatisticsPeriod.Daily => StatisticsInterval.Hour,
            StatisticsPeriod.Weekly => StatisticsInterval.Day,
            StatisticsPeriod.Monthly => StatisticsInterval.Day,
            StatisticsPeriod.Last7Days => StatisticsInterval.Day,
            StatisticsPeriod.Last30Days => StatisticsInterval.Day,
            StatisticsPeriod.Custom => ResolveCustomInterval(range),
            _ => throw new ArgumentException("Invalid statistics period")
        };
    }

    private static StatisticsInterval ResolveCustomInterval(StatisticsRange range)
    {
        var totalDays = range.EndDate.DayNumber - range.StartDate.DayNumber + 1;

        if (totalDays == 1)
        {
            return StatisticsInterval.Hour;
        }

        if (totalDays <= 31)
        {
            return StatisticsInterval.Day;
        }

        var firstWeekStart = range.StartDate;

        while (firstWeekStart.DayOfWeek != DayOfWeek.Monday)
        {
            firstWeekStart = firstWeekStart.AddDays(-1);
        }

        var calendarWeekCount = (range.EndDate.DayNumber - firstWeekStart.DayNumber) / 7 + 1;

        return calendarWeekCount <= 8
            ? StatisticsInterval.Week
            : StatisticsInterval.Month;
    }
}