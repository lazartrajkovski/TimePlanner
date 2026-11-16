using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Validators;

namespace TimePlanner.Services.Helpers;

public static class StatisticsRangeResolver
{
    public static StatisticsRange GetRangeUtc(StatisticsRequest request, TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTimeZone);
        var userLocalDate = DateOnly.FromDateTime(userLocalTime);

        if (request.Period == StatisticsPeriod.Custom)
        {
            var customStartDate = request.From!.Value;
            var customEndDate = request.To!.Value;

            StatisticsValidator.EnsureNotInFuture(customEndDate, userLocalDate);

            return ConvertToUtcRange(customStartDate, customEndDate, userTimeZone);
        }

        var rangeStartDate = request.Period switch
        {
            StatisticsPeriod.Daily => userLocalDate,
            StatisticsPeriod.Weekly => GetWeekStartDate(userLocalDate),
            StatisticsPeriod.Monthly => new DateOnly(userLocalDate.Year, userLocalDate.Month, 1),
            StatisticsPeriod.Last7Days => userLocalDate.AddDays(-6),
            StatisticsPeriod.Last30Days => userLocalDate.AddDays(-29),
            _ => throw new ArgumentException("Invalid statistics period")
        };

        return ConvertToUtcRange(rangeStartDate, userLocalDate, userTimeZone);
    }

    private static DateOnly GetWeekStartDate(DateOnly userLocalDate)
    {
        var weekStartDate = userLocalDate;

        while (weekStartDate.DayOfWeek != DayOfWeek.Monday)
        {
            weekStartDate = weekStartDate.AddDays(-1);
        }

        return weekStartDate;
    }

    private static StatisticsRange ConvertToUtcRange(DateOnly rangeStartDate, DateOnly rangeEndDate, TimeZoneInfo userTimeZone)
    {
        var localRangeStart = rangeStartDate.ToDateTime(TimeOnly.MinValue);
        var localRangeEnd = rangeEndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var utcRangeStart = TimeZoneInfo.ConvertTimeToUtc(localRangeStart, userTimeZone);
        var utcRangeEnd = TimeZoneInfo.ConvertTimeToUtc(localRangeEnd, userTimeZone);

        return new StatisticsRange
        {
            StartUtc = utcRangeStart,
            EndUtc = utcRangeEnd,
        };
    }
}