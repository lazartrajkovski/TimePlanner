using System.Text.Json.Serialization;
using TimePlanner.Domain.Enums;

namespace TimePlanner.Services.DTOs;

public class StatisticsResponse
{
    public StatisticsPeriod Period { get; set; }
    public DateOnly RangeStartDate { get; set; }
    public DateOnly RangeEndDate { get; set; }
    public long TotalTrackedSeconds { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<HourlyTrackedTimeResponse>? HourlyTrackedTime { get; set; }
}