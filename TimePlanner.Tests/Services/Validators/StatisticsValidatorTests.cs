using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs.Statistics;
using TimePlanner.Services.Validators;

namespace TimePlanner.Tests.Services.Validators;

public class StatisticsValidatorTests
{
    [Fact]
    public void Validate_InvalidPeriod_ThrowsArgumentException()
    {
        var request = new StatisticsRequest
        {
            Period = (StatisticsPeriod)999,
        };

        var exception = Assert.Throws<ArgumentException>(()
            => StatisticsValidator.Validate(request));

        Assert.Equal("Invalid statistics period", exception.Message);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Validate_CustomPeriodWithMissingDate_ThrowsArgumentException(bool includeFrom, bool includeTo)
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = includeFrom
                ? new DateOnly(2026, 11, 1)
                : null,
            To = includeTo
                ? new DateOnly(2026, 11, 19)
                : null,
        };

        var exception = Assert.Throws<ArgumentException>(()
            => StatisticsValidator.Validate(request));

        Assert.Equal("From and To are required for a custom period", exception.Message);
    }

    [Fact]
    public void Validate_CustomPeriodWithFromAfterTo_ThrowsArgumentException()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 20),
            To = new DateOnly(2026, 11, 19),
        };

        var exception = Assert.Throws<ArgumentException>(() => StatisticsValidator.Validate(request));

        Assert.Equal("From cannot be after To", exception.Message);
    }

    [Fact]
    public void Validate_CustomPeriodLongerThanMaximum_ThrowsArgumentException()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2027, 1, 2),
        };

        var exception = Assert.Throws<ArgumentException>(()
            => StatisticsValidator.Validate(request));

        Assert.Equal("Custom period cannot be longer than 366 days", exception.Message);
    }

    [Fact]
    public void Validate_CustomPeriodAtMaximumLength_DoesNotThrowException()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2027, 1, 1),
        };

        var exception = Record.Exception(()
            => StatisticsValidator.Validate(request));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(StatisticsPeriod.Daily)]
    [InlineData(StatisticsPeriod.Weekly)]
    [InlineData(StatisticsPeriod.Monthly)]
    [InlineData(StatisticsPeriod.Last7Days)]
    [InlineData(StatisticsPeriod.Last30Days)]
    public void Validate_NonCustomPeriodWithoutDates_DoesNotThrowException(StatisticsPeriod period)
    {
        var request = new StatisticsRequest
        {
            Period = period,
        };

        var exception = Record.Exception(() =>
            StatisticsValidator.Validate(request));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureNotInFuture_ToAfterUserLocalDate_ThrowsArgumentException()
    {
        var to = new DateOnly(2026, 11, 20);
        var userLocalDate = new DateOnly(2026, 11, 19);

        var exception = Assert.Throws<ArgumentException>(()
            => StatisticsValidator.EnsureNotInFuture(to, userLocalDate));

        Assert.Equal("To cannot be in the future", exception.Message);
    }

    [Fact]
    public void EnsureNotInFuture_ToEqualsUserLocalDate_DoesNotThrow()
    {
        var to = new DateOnly(2026, 11, 19);
        var userLocalDate = new DateOnly(2026, 11, 19);

        var exception = Record.Exception(()
            => StatisticsValidator.EnsureNotInFuture(to, userLocalDate));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_CustomPeriodForSingleDay_DoesNotThrowException()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 19),
            To = new DateOnly(2026, 11, 19),
        };

        var exception = Record.Exception(()
            => StatisticsValidator.Validate(request));

        Assert.Null(exception);
    }
}