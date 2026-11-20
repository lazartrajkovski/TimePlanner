using TimePlanner.Domain.Entities;
using TimePlanner.Services.Helpers;

namespace TimePlanner.Tests.Services.Statistics;

public class StatisticsCalculatorTests
{
    [Fact]
    public void GetTrackedSeconds_EntryInsideRange_ReturnsFullDuration()
    {
        var timeEntry = new TimeEntry
        {
            StartedAt = new DateTime(2026, 11, 19, 10, 30, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 19, 11, 15, 0, DateTimeKind.Utc),
        };

        var range = new StatisticsRange
        {
            StartUtc = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var utcNow = new DateTime(2026, 11, 19, 13, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.GetTrackedSeconds(timeEntry, range, utcNow);

        Assert.Equal(2700, result);
    }

    [Fact]
    public void GetTrackedSeconds_EntryStartsBeforeRange_CountsFromRangeStart()
    {
        var timeEntry = new TimeEntry
        {
            StartedAt = new DateTime(2026, 11, 19, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 19, 11, 0, 0, DateTimeKind.Utc),
        };

        var range = new StatisticsRange
        {
            StartUtc = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var utcNow = new DateTime(2026, 11, 19, 13, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.GetTrackedSeconds(timeEntry, range, utcNow);

        Assert.Equal(3600, result);
    }

    [Fact]
    public void GetTrackedSeconds_EntryEndsAfterRange_CountsUntilRangeEnd()
    {
        var timeEntry = new TimeEntry
        {
            StartedAt = new DateTime(2026, 11, 19, 11, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 19, 13, 0, 0, DateTimeKind.Utc),
        };

        var range = new StatisticsRange
        {
            StartUtc = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var utcNow = new DateTime(2026, 11, 19, 14, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.GetTrackedSeconds(timeEntry, range, utcNow);

        Assert.Equal(3600, result);
    }

    [Fact]
    public void GetTrackedSeconds_RunningEntry_CountsUntilUtcNow()
    {
        var timeEntry = new TimeEntry
        {
            StartedAt = new DateTime(2026, 11, 19, 10, 30, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        var range = new StatisticsRange
        {
            StartUtc = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var utcNow = new DateTime(2026, 11, 19, 11, 15, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.GetTrackedSeconds(timeEntry, range, utcNow);

        Assert.Equal(2700, result);
    }

    [Fact]
    public void GetTrackedSeconds_RunningEntryContinuesAfterRange_CountsUntilRangeEnd()
    {
        var timeEntry = new TimeEntry
        {
            StartedAt = new DateTime(2026, 11, 19, 11, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        var range = new StatisticsRange
        {
            StartUtc = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var utcNow = new DateTime(2026, 11, 19, 13, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.GetTrackedSeconds(timeEntry, range, utcNow);

        Assert.Equal(3600, result);
    }

    [Fact]
    public void GetTrackedSeconds_EntryEndsAtRangeStart_ReturnsZero()
    {
        var timeEntry = new TimeEntry
        {
            StartedAt = new DateTime(2026, 11, 19, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
        };

        var range = new StatisticsRange
        {
            StartUtc = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var utcNow = new DateTime(2026, 11, 19, 13, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.GetTrackedSeconds(timeEntry, range, utcNow);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetTrackedSeconds_EntryStartsAtRangeEnd_ReturnsZero()
    {
        var timeEntry = new TimeEntry
        {
            StartedAt = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 19, 13, 0, 0, DateTimeKind.Utc),
        };

        var range = new StatisticsRange
        {
            StartUtc = new DateTime(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc),
        };

        var utcNow = new DateTime(2026, 11, 19, 14, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.GetTrackedSeconds(timeEntry, range, utcNow);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateHourlyTrackedTime_EntryInsideSingleHour_ReturnsLocalHourInterval()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 11, 19, 9, 20, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 19, 9, 22, 0, DateTimeKind.Utc),
            }
        };

        var dayRange = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
            StartUtc = new DateTime(2026, 11, 18, 23, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateHourlyTrackedTime(timeEntries, dayRange, userTimeZone, utcNow);

        var interval = Assert.Single(result);

        Assert.Equal(new DateOnly(2026, 11, 19), interval.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 19), interval.EndDate);
        Assert.Equal(10, interval.Hour);
        Assert.Equal(120, interval.TrackedSeconds);
    }

    [Fact]
    public void CalculateHourlyTrackedTime_EntryCrossesHourBoundary_SplitsTrackedTime()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 11, 19, 9, 50, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 19, 10, 10, 0, DateTimeKind.Utc),
            }
        };

        var dayRange = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
            StartUtc = new DateTime(2026, 11, 18, 23, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateHourlyTrackedTime(timeEntries, dayRange, userTimeZone, utcNow);

        Assert.Collection(result, firstInterval =>
            {
                Assert.Equal(10, firstInterval.Hour);
                Assert.Equal(600, firstInterval.TrackedSeconds);
            },
            secondInterval =>
            {
                Assert.Equal(11, secondInterval.Hour);
                Assert.Equal(600, secondInterval.TrackedSeconds);
            });
    }

    [Fact]
    public void CalculateHourlyTrackedTime_MultipleEntriesInSameHour_ReturnsCombinedTime()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 11, 19, 9, 10, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 19, 9, 20, 0, DateTimeKind.Utc),
            },
            new()
            {
                StartedAt = new DateTime(2026, 11, 19, 9, 30, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 19, 9, 45, 0, DateTimeKind.Utc),
            }
        };

        var dayRange = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
            StartUtc = new DateTime(2026, 11, 18, 23, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateHourlyTrackedTime(timeEntries, dayRange, userTimeZone, utcNow);

        var interval = Assert.Single(result);

        Assert.Equal(10, interval.Hour);
        Assert.Equal(1500, interval.TrackedSeconds);
    }

    [Fact]
    public void CalculateHourlyTrackedTime_NoEntries_ReturnsEmptyList()
    {
        IReadOnlyList<TimeEntry> timeEntries = [];

        var dayRange = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 19),
            EndDate = new DateOnly(2026, 11, 19),
            StartUtc = new DateTime(2026, 11, 18, 23, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 19, 23, 0, 0, DateTimeKind.Utc),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateHourlyTrackedTime(timeEntries, dayRange, userTimeZone, utcNow);

        Assert.Empty(result);
    }

    [Fact]
    public void CalculateHourlyTrackedTime_RepeatedAutumnHour_CombinesBothRealHours()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 10, 25, 0, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 10, 25, 2, 0, 0, DateTimeKind.Utc),
            }
        };

        var dayRange = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 10, 25),
            EndDate = new DateOnly(2026, 10, 25),
            StartUtc = new DateTime(2026, 10, 24, 22, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 10, 25, 23, 0, 0, DateTimeKind.Utc),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateHourlyTrackedTime(timeEntries, dayRange, userTimeZone, utcNow);

        var interval = Assert.Single(result);

        Assert.Equal(2, interval.Hour);
        Assert.Equal(7200, interval.TrackedSeconds);
    }

    [Fact]
    public void CalculateHourlyTrackedTime_SkippedSpringHour_ReturnsOnlyRealLocalHours()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 3, 29, 0, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 3, 29, 2, 0, 0, DateTimeKind.Utc),
            }
        };

        var dayRange = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 3, 29),
            EndDate = new DateOnly(2026, 3, 29),
            StartUtc = new DateTime(2026, 3, 28, 23, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 3, 29, 22, 0, 0, DateTimeKind.Utc),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 3, 29, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateHourlyTrackedTime(timeEntries, dayRange, userTimeZone, utcNow);

        Assert.Collection(result, firstInterval =>
            {
                Assert.Equal(1, firstInterval.Hour);
                Assert.Equal(3600, firstInterval.TrackedSeconds);
            },
            secondInterval =>
            {
                Assert.Equal(3, secondInterval.Hour);
                Assert.Equal(3600, secondInterval.TrackedSeconds);
            });
    }

    [Fact]
    public void CalculateDailyTrackedTime_MultipleDays_ReturnsEveryDayIncludingEmptyDays()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                StartedAt = new DateTime(2026, 11, 19, 11, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 19, 11, 30, 0, DateTimeKind.Utc),
            }
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 17),
            EndDate = new DateOnly(2026, 11, 19),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateDailyTrackedTime(timeEntries, range, userTimeZone, utcNow);

        Assert.Collection(result, firstDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 17), firstDay.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 17), firstDay.EndDate);
                Assert.Equal(3600, firstDay.TrackedSeconds);
            },
            secondDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 18), secondDay.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 18), secondDay.EndDate);
                Assert.Equal(0, secondDay.TrackedSeconds);
            },
            thirdDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 19), thirdDay.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 19), thirdDay.EndDate);
                Assert.Equal(1800, thirdDay.TrackedSeconds);
            });
    }

    [Fact]
    public void CalculateDailyTrackedTime_EntryCrossesMidnight_SplitsTimeBetweenDays()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 11, 17, 22, 30, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 17, 23, 30, 0, DateTimeKind.Utc),
            }
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 17),
            EndDate = new DateOnly(2026, 11, 18),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateDailyTrackedTime(timeEntries, range, userTimeZone, utcNow);

        Assert.Collection(result, firstDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 17), firstDay.StartDate);
                Assert.Equal(1800, firstDay.TrackedSeconds);
            },
            secondDay =>
            {
                Assert.Equal(new DateOnly(2026, 11, 18), secondDay.StartDate);
                Assert.Equal(1800, secondDay.TrackedSeconds);
            });
    }

    [Fact]
    public void CalculateWeeklyTrackedTime_RangeStartsAndEndsMidweek_ReturnsClippedWeeks()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 11, 5, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 5, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                StartedAt = new DateTime(2026, 11, 16, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc),
            }
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 4),
            EndDate = new DateOnly(2026, 11, 17),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateWeeklyTrackedTime(timeEntries, range, userTimeZone, utcNow);

        Assert.Collection(result, firstWeek =>
            {
                Assert.Equal(new DateOnly(2026, 11, 4), firstWeek.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 8), firstWeek.EndDate);
                Assert.Equal(3600, firstWeek.TrackedSeconds);
            },
            secondWeek =>
            {
                Assert.Equal(new DateOnly(2026, 11, 9), secondWeek.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 15), secondWeek.EndDate);
                Assert.Equal(0, secondWeek.TrackedSeconds);
            },
            thirdWeek =>
            {
                Assert.Equal(new DateOnly(2026, 11, 16), thirdWeek.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 17), thirdWeek.EndDate);
                Assert.Equal(3600, thirdWeek.TrackedSeconds);
            });
    }

    [Fact]
    public void CalculateWeeklyTrackedTime_EntryCrossesWeekBoundary_SplitsTimeBetweenWeeks()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 11, 15, 22, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc),
            }
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 11, 9),
            EndDate = new DateOnly(2026, 11, 22),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateWeeklyTrackedTime(timeEntries, range, userTimeZone, utcNow);

        Assert.Collection(result, firstWeek =>
            {
                Assert.Equal(new DateOnly(2026, 11, 9), firstWeek.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 15), firstWeek.EndDate);
                Assert.Equal(3600, firstWeek.TrackedSeconds);
            },
            secondWeek =>
            {
                Assert.Equal(new DateOnly(2026, 11, 16), secondWeek.StartDate);
                Assert.Equal(new DateOnly(2026, 11, 22), secondWeek.EndDate);
                Assert.Equal(3600, secondWeek.TrackedSeconds);
            });
    }

    [Fact]
    public void CalculateMonthlyTrackedTime_RangeStartsAndEndsMidmonth_ReturnsClippedMonths()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 1, 20, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 1, 20, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                StartedAt = new DateTime(2026, 3, 5, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc),
            }
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 1, 15),
            EndDate = new DateOnly(2026, 3, 10),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateMonthlyTrackedTime(timeEntries, range, userTimeZone, utcNow);

        Assert.Collection(result, january =>
            {
                Assert.Equal(new DateOnly(2026, 1, 15), january.StartDate);
                Assert.Equal(new DateOnly(2026, 1, 31), january.EndDate);
                Assert.Equal(3600, january.TrackedSeconds);
            },
            february =>
            {
                Assert.Equal(new DateOnly(2026, 2, 1), february.StartDate);
                Assert.Equal(new DateOnly(2026, 2, 28), february.EndDate);
                Assert.Equal(0, february.TrackedSeconds);
            },
            march =>
            {
                Assert.Equal(new DateOnly(2026, 3, 1), march.StartDate);
                Assert.Equal(new DateOnly(2026, 3, 10), march.EndDate);
                Assert.Equal(3600, march.TrackedSeconds);
            });
    }

    [Fact]
    public void CalculateMonthlyTrackedTime_EntryCrossesMonthBoundary_SplitsTimeBetweenMonths()
    {
        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                StartedAt = new DateTime(2026, 1, 31, 22, 30, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 1, 31, 23, 30, 0, DateTimeKind.Utc),
            }
        };

        var range = new StatisticsRange
        {
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 2, 28),
        };

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        var utcNow = new DateTime(2026, 2, 28, 12, 0, 0, DateTimeKind.Utc);

        var result = StatisticsCalculator.CalculateMonthlyTrackedTime(timeEntries, range, userTimeZone, utcNow);

        Assert.Collection(result, january =>
            {
                Assert.Equal(new DateOnly(2026, 1, 1), january.StartDate);
                Assert.Equal(new DateOnly(2026, 1, 31), january.EndDate);
                Assert.Equal(1800, january.TrackedSeconds);
            },
            february =>
            {
                Assert.Equal(new DateOnly(2026, 2, 1), february.StartDate);
                Assert.Equal(new DateOnly(2026, 2, 28), february.EndDate);
                Assert.Equal(1800, february.TrackedSeconds);
            });
    }
}