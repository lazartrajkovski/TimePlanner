using TimePlanner.Services.DTOs.Statistics;

namespace TimePlanner.Services.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsResponse> GetStatistics(StatisticsRequest request);
}