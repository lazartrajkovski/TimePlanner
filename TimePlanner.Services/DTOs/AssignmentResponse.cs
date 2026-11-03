using TimePlanner.Domain.Enums;

namespace TimePlanner.Services.DTOs;

public class AssignmentResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string AssignmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssignmentStatus Status { get; set; }
    public double SpentHours { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}