namespace TimePlanner.Domain.Entities;

public class TimeEntry
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public Assignment Assignment { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}