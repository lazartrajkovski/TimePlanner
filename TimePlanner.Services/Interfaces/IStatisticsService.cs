using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsResponse> GetStatistics(StatisticsRequest request);
}