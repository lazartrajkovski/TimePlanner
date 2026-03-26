namespace TimePlanner.Services.DTOs
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<AssignmentResponse> Assignments { get; set; }  = new List<AssignmentResponse>();
    }
}
