using TimePlanner.Domain.Enums;

namespace TimePlanner.Services.DTOs.Statistics;

public class StatisticsResponse
{
    public StatisticsPeriod Period { get; set; }
    public DateOnly RangeStartDate { get; set; }
    public DateOnly RangeEndDate { get; set; }
    public long TotalTrackedSeconds { get; set; }
    public List<TrackedTimeIntervalResponse> TrackedTime { get; set; } = [];
    public StatisticsInterval Interval { get; set; }
}