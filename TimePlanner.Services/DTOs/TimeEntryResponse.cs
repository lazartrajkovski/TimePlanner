namespace TimePlanner.Services.DTOs;

public class TimeEntryResponse
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationTime { get; set; }
}