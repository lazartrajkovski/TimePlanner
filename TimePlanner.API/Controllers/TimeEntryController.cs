using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TimeEntryController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;

    public TimeEntryController(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<TimeEntryResponse>> Start(StartTimeEntryRequest request)
    {
        var response = await _timeEntryService.Start(request);
        return Ok(response);
    }

    [HttpPatch("stop")]
    public async Task<ActionResult<TimeEntryResponse>> Stop()
    {
        var response = await _timeEntryService.Stop();
        return Ok(response);
    }

    [HttpGet("active")]
    public async Task<ActionResult<TimeEntryResponse>> GetActive()
    {
        var response = await _timeEntryService.GetActiveTimeEntry();

        if (response is null)
        {
            return NoContent();
        }

        return Ok(response);
    }

    [HttpGet("assignment/{assignmentId:int}")]
    public async Task<ActionResult<IEnumerable<TimeEntryResponse>>> GetByAssignmentId(int assignmentId)
    {
        var response = await _timeEntryService.GetByAssignmentId(assignmentId);
        return Ok(response);
    }
}