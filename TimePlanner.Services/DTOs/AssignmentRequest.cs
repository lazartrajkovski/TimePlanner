using TimePlanner.Domain.Enums;

namespace TimePlanner.Services.DTOs
{
    public class AssignmentRequest
    {
        public int CategoryId { get; set; }
        public required string AssignmentName { get; set; }
        public string? Description { get; set; }
        public double? EstimatedHours { get; set; }
        public DateTime? DueDate { get; set; }
        public AssignmentType Type { get; set; }
    }
}
