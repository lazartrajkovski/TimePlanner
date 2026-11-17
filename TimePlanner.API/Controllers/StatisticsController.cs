using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimePlanner.Services.DTOs.Statistics;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet]
    public async Task<ActionResult<StatisticsResponse>> GetStatistics([FromQuery] StatisticsRequest request)
    {
        var statistics = await _statisticsService.GetStatistics(request);
        return Ok(statistics);
    }
}