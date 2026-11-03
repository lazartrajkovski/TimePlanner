namespace TimePlanner.Services.DTOs;

public class AssignmentRequest
{
    public int CategoryId { get; set; }
    public required string AssignmentName { get; set; }
    public string? Description { get; set; }
    public DateOnly? DueDate { get; set; }
}