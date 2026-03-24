namespace TimePlanner.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public ICollection<Assignment> Tasks { get; set; } = new List<Assignment>();
    }
}
