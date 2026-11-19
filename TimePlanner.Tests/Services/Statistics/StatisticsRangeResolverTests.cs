using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs.Statistics;
using TimePlanner.Services.Helpers;

namespace TimePlanner.Tests.Services.Statistics;

public class StatisticsRangeResolverTests
{
    [Fact]
    public void GetRangeUtc_DailyPeriod_ReturnsCurrentLocalDay()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Daily
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 11, 19), result.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 19), result.EndDate);
        Assert.Equal(new DateTime(2026, 11, 18, 23, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
    }

    [Fact]
    public void GetRangeUtc_WeeklyPeriod_ReturnsMondayThroughCurrentLocalDay()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Weekly
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 11, 16), result.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 19), result.EndDate);
        Assert.Equal(new DateTime(2026, 11, 15, 23, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
    }

    [Fact]
    public void GetRangeUtc_MonthlyPeriod_ReturnsFirstDayThroughCurrentLocalDay()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Monthly
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 11, 1), result.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 19), result.EndDate);
        Assert.Equal(new DateTime(2026, 10, 31, 23, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
    }

    [Fact]
    public void GetRangeUtc_Last7DaysPeriod_ReturnsExpectedDateRange()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Last7Days,
        };

        var userTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(
            2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 11, 13), result.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 19), result.EndDate);
        Assert.Equal(new DateTime(2026, 11, 12, 23, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
    }

    [Fact]
    public void GetRangeUtc_Last30DaysPeriod_ReturnsExpectedDateRange()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Last30Days,
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 10, 21), result.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 19), result.EndDate);
        Assert.Equal(new DateTime(2026, 10, 20, 22, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
    }

    [Fact]
    public void GetRangeUtc_CustomPeriod_ReturnsRequestedDateRange()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 13),
            To = new DateOnly(2026, 11, 15),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 11, 13), result.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 15), result.EndDate);
        Assert.Equal(new DateTime(2026, 11, 12, 23, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 11, 15, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
    }

    [Fact]
    public void GetRangeUtc_CustomPeriodEndingOnCurrentLocalDay_ReturnsRequestedDateRange()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 17),
            To = new DateOnly(2026, 11, 19),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 11, 17), result.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 19), result.EndDate);
        Assert.Equal(new DateTime(2026, 11, 16, 23, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
    }

    [Fact]
    public void GetRangeUtc_CustomPeriodEndingInFuture_ThrowsArgumentException()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 19),
            To = new DateOnly(2026, 11, 20),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(
            2026, 11, 18, 23, 30, 0,
            DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentException>(() =>
            StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow));

        Assert.Equal("To cannot be in the future", exception.Message);
    }

    [Fact]
    public void GetRangeUtc_DailyPeriodOnSpringClockChange_ReturnsTwentyThreeHourRange()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Daily,
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 3, 29, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 3, 29), result.StartDate);
        Assert.Equal(new DateOnly(2026, 3, 29), result.EndDate);
        Assert.Equal(new DateTime(2026, 3, 28, 23, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 3, 29, 22, 0, 0, DateTimeKind.Utc), result.EndUtc);
        Assert.Equal(TimeSpan.FromHours(23), result.EndUtc - result.StartUtc);
    }

    [Fact]
    public void GetRangeUtc_DailyPeriodOnAutumnClockChange_ReturnsTwentyFiveHourRange()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Daily,
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow);

        Assert.Equal(new DateOnly(2026, 10, 25), result.StartDate);
        Assert.Equal(new DateOnly(2026, 10, 25), result.EndDate);
        Assert.Equal(new DateTime(2026, 10, 24, 22, 0, 0, DateTimeKind.Utc), result.StartUtc);
        Assert.Equal(new DateTime(2026, 10, 25, 23, 0, 0, DateTimeKind.Utc), result.EndUtc);
        Assert.Equal(TimeSpan.FromHours(25), result.EndUtc - result.StartUtc);
    }

    [Fact]
    public void GetRangeUtc_InvalidPeriod_ThrowsArgumentException()
    {
        var request = new StatisticsRequest
        {
            Period = (StatisticsPeriod)999,
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");

        var utcNow = new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentException>(() =>
            StatisticsRangeResolver.GetRangeUtc(request, userTimeZone, utcNow));

        Assert.Equal("Invalid statistics period", exception.Message);
    }
}