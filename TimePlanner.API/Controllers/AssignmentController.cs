using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentResponse>> Create(AssignmentRequest request)
    {
        var response = await _assignmentService.Add(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentResponse>>> GetAll([FromQuery] AssignmentStatus? status)
    {
        var response = await _assignmentService.GetAll(status);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(int id)
    {
        var response = await _assignmentService.GetById(id);
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AssignmentResponse>> Update(int id, AssignmentRequest request)
    {
        var response = await _assignmentService.Update(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _assignmentService.Delete(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<AssignmentResponse>> UpdateStatus(int id, [FromQuery] AssignmentStatus status)
    {
        var newStatus = await _assignmentService.ChangeStatus(id, status);
        return Ok(newStatus);
    }

    [HttpGet("category/{categoryId:int}")]
    public async Task<ActionResult<IEnumerable<AssignmentResponse>>> GetByCategoryId(int categoryId)
    {
        var assignments = await _assignmentService.GetByCategoryId(categoryId);
        return Ok(assignments);
    }

    [HttpPatch("{id:int}/archive")]
    public async Task<ActionResult> Archive(int id)
    {
        await _assignmentService.Archive(id);
        return NoContent();
    }

    [HttpGet("archived")]
    public async Task<ActionResult<IEnumerable<AssignmentResponse>>> GetArchived()
    {
        var archivedAssignments = await _assignmentService.GetArchivedByUserId();
        return Ok(archivedAssignments);
    }
}