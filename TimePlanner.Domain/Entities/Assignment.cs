using TimePlanner.Domain.Enums;

namespace TimePlanner.Domain.Entities;

public class Assignment
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string AssignmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}