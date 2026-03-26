using TimePlanner.Domain.Enums;

namespace TimePlanner.Services.DTOs
{
    public class AssignmentResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AssignmentType Type { get; set; }
        public AssignmentStatus Status { get; set; }
        public double EstimatedHours { get; set; }
        public double SpentHours { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
