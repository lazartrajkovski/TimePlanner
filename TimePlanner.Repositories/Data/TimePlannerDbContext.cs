using Microsoft.EntityFrameworkCore;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Configurations;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
        }
    }
}
