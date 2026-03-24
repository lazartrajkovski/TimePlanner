using Microsoft.EntityFrameworkCore;
using TimePlanner.Domain.Entities;

namespace TimePlanner.Repositories.Data
{
    public class TimePlannerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public TimePlannerDbContext(DbContextOptions<TimePlannerDbContext> options) : base(options)
        {

        }
    }
}
