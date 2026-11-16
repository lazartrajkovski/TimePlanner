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

        DateOnly rangeStartDate;
        DateOnly rangeEndDate = userLocalDate;

        if (request.Period == StatisticsPeriod.Daily)
        {
            rangeStartDate = userLocalDate;
            rangeEndDate = userLocalDate;
        }
        else if (request.Period == StatisticsPeriod.Weekly)
        {
            var weekStartDate = userLocalDate;

            while (weekStartDate.DayOfWeek != DayOfWeek.Monday)
            {
                weekStartDate = weekStartDate.AddDays(-1);
            }

            rangeStartDate = weekStartDate;
        }
        else if (request.Period == StatisticsPeriod.Monthly)
        {
            rangeStartDate = new DateOnly(
                userLocalDate.Year,
                userLocalDate.Month,
                1);
        }
        else if (request.Period == StatisticsPeriod.Last7Days)
        {
            rangeStartDate = userLocalDate.AddDays(-6);
        }
        else if (request.Period == StatisticsPeriod.Last30Days)
        {
            rangeStartDate = userLocalDate.AddDays(-29);
        }
        else if (request.Period == StatisticsPeriod.Custom)
        {
            rangeStartDate = request.From!.Value;
            rangeEndDate = request.To!.Value;

            StatisticsValidator.EnsureNotInFuture(rangeEndDate, userLocalDate);
        }
        else
        {
            throw new ArgumentException("Invalid statistics period");
        }

        var localDayStart = rangeStartDate.ToDateTime(TimeOnly.MinValue);
        var localDayEnd = rangeEndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var utcDayStart = TimeZoneInfo.ConvertTimeToUtc(localDayStart, userTimeZone);
        var utcDayEnd = TimeZoneInfo.ConvertTimeToUtc(localDayEnd, userTimeZone);

        return new StatisticsRange
        {
            StartUtc = utcDayStart,
            EndUtc = utcDayEnd,
        };
    }
}