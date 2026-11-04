using Microsoft.EntityFrameworkCore;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Configurations;

namespace TimePlanner.Repositories.Data;

public class TimePlannerDbContext : DbContext
{
    public TimePlannerDbContext(DbContextOptions<TimePlannerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new TimeEntryConfiguration());
    }
}