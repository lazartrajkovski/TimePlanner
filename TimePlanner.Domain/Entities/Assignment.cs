using TimePlanner.Domain.Enums;

namespace TimePlanner.Domain.Entities
{
    public class Assignment
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string AssignmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AssignmentType Type { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        public double EstimatedHours { get; set; }
        public double SpentHours { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Category Category { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
