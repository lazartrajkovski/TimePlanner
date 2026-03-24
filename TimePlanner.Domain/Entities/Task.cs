using TimePlanner.Domain.Enums;

namespace TimePlanner.Domain.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskType Type { get; set; }
        public TaskState Status { get; set; } = TaskState.Pending;
        public double EstimatedHours { get; set; }
        public double SpentHours { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Category Category { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
