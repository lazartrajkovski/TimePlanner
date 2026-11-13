using TimePlanner.Domain.Enums;

namespace TimePlanner.Services.DTOs;

public class StatisticsRequest
{
    public StatisticsPeriod Period { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}