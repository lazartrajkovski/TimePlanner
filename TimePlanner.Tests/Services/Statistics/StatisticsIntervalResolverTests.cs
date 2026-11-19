using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs.Statistics;
using TimePlanner.Services.Helpers;

namespace TimePlanner.Tests.Services.Statistics;

public class StatisticsIntervalResolverTests
{
    [Fact]
    public void Resolve_DailyPeriod_ReturnsHourInterval()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Daily
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Hour, result);
    }

    [Theory]
    [InlineData(StatisticsPeriod.Weekly)]
    [InlineData(StatisticsPeriod.Monthly)]
    [InlineData(StatisticsPeriod.Last7Days)]
    [InlineData(StatisticsPeriod.Last30Days)]
    public void Resolve_DayBasedPeriod_ReturnsDayInterval(StatisticsPeriod period)
    {
        var request = new StatisticsRequest
        {
            Period = period
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Day, result);
    }

    [Fact]
    public void Resolve_OneDayCustomRange_ReturnsHourInterval()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Hour, result);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(31)]
    public void Resolve_CustomRangeUpToThirtyOneDays_ReturnsDayInterval(int totalDays)
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom
        };

        var startDate = new DateOnly(2026, 10, 1);

        var range = new StatisticsRange
        {
            StartDate = startDate,
            EndDate = startDate.AddDays(totalDays - 1)
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Day, result);
    }

    [Theory]
    [InlineData(32)]
    [InlineData(56)]
    public void Resolve_CustomRangeUpToEightWeeks_ReturnsWeekInterval(int totalDays)
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom
        };

        var startDate = new DateOnly(2026, 1, 5);

        var range = new StatisticsRange
        {
            StartDate = startDate,
            EndDate = startDate.AddDays(totalDays - 1)
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Week, result);
    }

    [Fact]
    public void Resolve_CustomRangeTouchingEightCalendarWeeksFromMidweek_ReturnsWeekInterval()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 1, 8),
            EndDate = new DateOnly(2026, 3, 1),
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Week, result);
    }

    [Theory]
    [InlineData(57)]
    [InlineData(366)]
    public void Resolve_CustomRangeOverEightWeeks_ReturnsMonthInterval(int totalDays)
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom
        };

        var startDate = new DateOnly(2026, 1, 5);

        var range = new StatisticsRange
        {
            StartDate = startDate,
            EndDate = startDate.AddDays(totalDays - 1)
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Month, result);
    }

    [Fact]
    public void Resolve_CustomRangeTouchingNineCalendarWeeksFromMidweek_ReturnsMonthInterval()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 1, 4),
            EndDate = new DateOnly(2026, 2, 23),
        };

        var result = StatisticsIntervalResolver.Resolve(request, range);

        Assert.Equal(StatisticsInterval.Month, result);
    }

    [Fact]
    public void Resolve_InvalidPeriod_ThrowsArgumentException()
    {
        var request = new StatisticsRequest
        {
            Period = (StatisticsPeriod)999
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
        };

        var exception = Assert.Throws<ArgumentException>(()
            => StatisticsIntervalResolver.Resolve(request, range));

        Assert.Equal("Invalid statistics period", exception.Message);
    }
}