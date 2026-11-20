using Moq;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs.Statistics;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Services;

namespace TimePlanner.Tests.Services.Statistics;

public class StatisticsServiceTests
{
    private readonly Mock<ITimeEntryRepository> _timeEntryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly StatisticsService _statisticsService;

    public StatisticsServiceTests()
    {
        _timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserMock = new Mock<ICurrentUser>();
        _statisticsService = new StatisticsService(_timeEntryRepositoryMock.Object, _userRepositoryMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task GetStatistics_InvalidPeriod_ThrowsBeforeAccessingRepositories()
    {
        var request = new StatisticsRequest
        {
            Period = (StatisticsPeriod)999,
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _statisticsService.GetStatistics(request));

        Assert.Equal("Invalid statistics period", exception.Message);

        _currentUserMock.VerifyGet(currentUser => currentUser.UserId, Times.Never);

        _userRepositoryMock.Verify(repository => repository.GetById(It.IsAny<int>()), Times.Never);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetByUserIdAndDateRange(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task GetStatistics_UserNotFound_ThrowsBeforeLoadingTimeEntries()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Daily,
        };

        _currentUserMock.Setup(currentUser => currentUser.UserId).Returns(42);

        _userRepositoryMock.Setup(repository => repository.GetById(42)).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _statisticsService.GetStatistics(request));

        Assert.Equal("User not found", exception.Message);

        _userRepositoryMock.Verify(repository => repository.GetById(42), Times.Once);

        _timeEntryRepositoryMock.Verify(repository =>
                repository.GetByUserIdAndDateRange(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task GetStatistics_ValidOneDayCustomRequest_ReturnsHourlyStatistics()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 18),
            To = new DateOnly(2026, 11, 18),
        };

        var user = new User
        {
            Id = 42,
            TimeZoneId = "UTC",
        };

        IReadOnlyList<TimeEntry> timeEntries =
        [
            new()
        {
            UserId = 42,
            StartedAt = new DateTime(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 18, 10, 30, 0, DateTimeKind.Utc),
        }
        ];

        var rangeStartUtc = new DateTime(2026, 11, 18, 0, 0, 0, DateTimeKind.Utc);

        var rangeEndUtc = new DateTime(2026, 11, 19, 0, 0, 0, DateTimeKind.Utc);

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _userRepositoryMock
            .Setup(repository => repository.GetById(42))
            .ReturnsAsync(user);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc))
            .ReturnsAsync(timeEntries);

        var result = await _statisticsService.GetStatistics(request);

        Assert.Equal(StatisticsPeriod.Custom, result.Period);
        Assert.Equal(new DateOnly(2026, 11, 18), result.RangeStartDate);
        Assert.Equal(new DateOnly(2026, 11, 18), result.RangeEndDate);
        Assert.Equal(StatisticsInterval.Hour, result.Interval);
        Assert.Equal(1800, result.TotalTrackedSeconds);

        var interval = Assert.Single(result.TrackedTime);

        Assert.Equal(new DateOnly(2026, 11, 18), interval.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 18), interval.EndDate);
        Assert.Equal(10, interval.Hour);
        Assert.Equal(1800, interval.TrackedSeconds);

        _userRepositoryMock.Verify(
            repository => repository.GetById(42),
            Times.Once);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc),
            Times.Once);
    }

    [Fact]
    public async Task GetStatistics_MultiDayCustomRequest_ReturnsDailyStatistics()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 16),
            To = new DateOnly(2026, 11, 18),
        };

        var user = new User
        {
            Id = 42,
            TimeZoneId = "UTC",
        };

        IReadOnlyList<TimeEntry> timeEntries =
        [
            new()
            {
                UserId = 42,
                StartedAt = new DateTime(2026, 11, 16, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                UserId = 42,
                StartedAt = new DateTime(2026, 11, 18, 11, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 18, 11, 30, 0, DateTimeKind.Utc),
            },
        ];

        var rangeStartUtc = new DateTime(
            2026, 11, 16, 0, 0, 0,
            DateTimeKind.Utc);

        var rangeEndUtc = new DateTime(
            2026, 11, 19, 0, 0, 0,
            DateTimeKind.Utc);

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _userRepositoryMock
            .Setup(repository => repository.GetById(42))
            .ReturnsAsync(user);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc))
            .ReturnsAsync(timeEntries);

        var result = await _statisticsService.GetStatistics(request);

        Assert.Equal(StatisticsPeriod.Custom, result.Period);
        Assert.Equal(new DateOnly(2026, 11, 16), result.RangeStartDate);
        Assert.Equal(new DateOnly(2026, 11, 18), result.RangeEndDate);
        Assert.Equal(StatisticsInterval.Day, result.Interval);
        Assert.Equal(5400, result.TotalTrackedSeconds);

        Assert.Collection(
            result.TrackedTime,
            firstDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 16), firstDay.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 16), firstDay.EndDate);
                Assert.Equal(3600, firstDay.TrackedSeconds);
            },
            secondDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 17), secondDay.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 17), secondDay.EndDate);
                Assert.Equal(0, secondDay.TrackedSeconds);
            },
            thirdDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 18), thirdDay.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 18), thirdDay.EndDate);
                Assert.Equal(1800, thirdDay.TrackedSeconds);
            });

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc),
            Times.Once);
    }

    [Fact]
    public async Task GetStatistics_UserTimeZone_RequestsTimeEntriesUsingConvertedUtcRange()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 11, 18),
            To = new DateOnly(2026, 11, 18),
        };

        var user = new User
        {
            Id = 42,
            TimeZoneId = "Europe/Skopje",
        };

        IReadOnlyList<TimeEntry> timeEntries = [];

        var expectedRangeStartUtc = new DateTime(
            2026, 11, 17, 23, 0, 0,
            DateTimeKind.Utc);

        var expectedRangeEndUtc = new DateTime(
            2026, 11, 18, 23, 0, 0,
            DateTimeKind.Utc);

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _userRepositoryMock
            .Setup(repository => repository.GetById(42))
            .ReturnsAsync(user);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetByUserIdAndDateRange(
                42,
                expectedRangeStartUtc,
                expectedRangeEndUtc))
            .ReturnsAsync(timeEntries);

        var result = await _statisticsService.GetStatistics(request);

        Assert.Equal(new DateOnly(2026, 11, 18), result.RangeStartDate);
        Assert.Equal(new DateOnly(2026, 11, 18), result.RangeEndDate);
        Assert.Equal(StatisticsInterval.Hour, result.Interval);
        Assert.Empty(result.TrackedTime);
        Assert.Equal(0, result.TotalTrackedSeconds);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetByUserIdAndDateRange(
                42,
                expectedRangeStartUtc,
                expectedRangeEndUtc),
            Times.Once);
    }

    [Fact]
    public async Task GetStatistics_CustomRangeUpToEightCalendarWeeks_ReturnsWeeklyStatistics()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 10, 1),
            To = new DateOnly(2026, 11, 17),
        };

        var user = new User
        {
            Id = 42,
            TimeZoneId = "UTC",
        };

        IReadOnlyList<TimeEntry> timeEntries = [];

        var rangeStartUtc = new DateTime(
            2026, 10, 1, 0, 0, 0,
            DateTimeKind.Utc);

        var rangeEndUtc = new DateTime(
            2026, 11, 18, 0, 0, 0,
            DateTimeKind.Utc);

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _userRepositoryMock
            .Setup(repository => repository.GetById(42))
            .ReturnsAsync(user);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc))
            .ReturnsAsync(timeEntries);

        var result = await _statisticsService.GetStatistics(request);

        Assert.Equal(StatisticsPeriod.Custom, result.Period);
        Assert.Equal(new DateOnly(2026, 10, 1), result.RangeStartDate);
        Assert.Equal(new DateOnly(2026, 11, 17), result.RangeEndDate);
        Assert.Equal(StatisticsInterval.Week, result.Interval);
        Assert.Equal(0, result.TotalTrackedSeconds);
        Assert.Equal(8, result.TrackedTime.Count);

        var firstInterval = result.TrackedTime.First();
        var lastInterval = result.TrackedTime.Last();

        Assert.Equal(new DateOnly(2026, 10, 1), firstInterval.StartDate);
        Assert.Equal(new DateOnly(2026, 10, 4), firstInterval.EndDate);

        Assert.Equal(new DateOnly(2026, 11, 16), lastInterval.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 17), lastInterval.EndDate);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc),
            Times.Once);
    }

    [Fact]
    public async Task GetStatistics_LongCustomRange_ReturnsMonthlyStatistics()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Custom,
            From = new DateOnly(2026, 1, 15),
            To = new DateOnly(2026, 11, 17),
        };

        var user = new User
        {
            Id = 42,
            TimeZoneId = "UTC",
        };

        IReadOnlyList<TimeEntry> timeEntries = [];

        var rangeStartUtc = new DateTime(
            2026, 1, 15, 0, 0, 0,
            DateTimeKind.Utc);

        var rangeEndUtc = new DateTime(
            2026, 11, 18, 0, 0, 0,
            DateTimeKind.Utc);

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _userRepositoryMock
            .Setup(repository => repository.GetById(42))
            .ReturnsAsync(user);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc))
            .ReturnsAsync(timeEntries);

        var result = await _statisticsService.GetStatistics(request);

        Assert.Equal(StatisticsPeriod.Custom, result.Period);
        Assert.Equal(new DateOnly(2026, 1, 15), result.RangeStartDate);
        Assert.Equal(new DateOnly(2026, 11, 17), result.RangeEndDate);
        Assert.Equal(StatisticsInterval.Month, result.Interval);
        Assert.Equal(0, result.TotalTrackedSeconds);
        Assert.Equal(11, result.TrackedTime.Count);

        var firstInterval = result.TrackedTime.First();
        var lastInterval = result.TrackedTime.Last();

        Assert.Equal(new DateOnly(2026, 1, 15), firstInterval.StartDate);
        Assert.Equal(new DateOnly(2026, 1, 31), firstInterval.EndDate);

        Assert.Equal(new DateOnly(2026, 11, 1), lastInterval.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 17), lastInterval.EndDate);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetByUserIdAndDateRange(
                42,
                rangeStartUtc,
                rangeEndUtc),
            Times.Once);
    }

    [Fact]
    public async Task GetStatistics_DailyRequest_ReturnsDailyPeriodWithHourlyInterval()
    {
        var request = new StatisticsRequest
        {
            Period = StatisticsPeriod.Daily,
        };

        var user = new User
        {
            Id = 42,
            TimeZoneId = "UTC",
        };

        IReadOnlyList<TimeEntry> timeEntries = [];

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _userRepositoryMock
            .Setup(repository => repository.GetById(42))
            .ReturnsAsync(user);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetByUserIdAndDateRange(
                42,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(timeEntries);

        var result = await _statisticsService.GetStatistics(request);

        Assert.Equal(StatisticsPeriod.Daily, result.Period);
        Assert.Equal(StatisticsInterval.Hour, result.Interval);
        Assert.Equal(result.RangeStartDate, result.RangeEndDate);
        Assert.Equal(0, result.TotalTrackedSeconds);
        Assert.Empty(result.TrackedTime);

        _userRepositoryMock.Verify(
            repository => repository.GetById(42),
            Times.Once);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetByUserIdAndDateRange(
                42,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()),
            Times.Once);
    }
}