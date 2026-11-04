using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimePlanner.Domain.Entities;

namespace TimePlanner.Repositories.Configurations;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.HasKey(te => te.Id);

        builder.HasOne(te => te.Assignment)
            .WithMany(a => a.TimeEntries)
            .HasForeignKey(te => te.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(te => te.User)
            .WithMany(u => u.TimeEntries)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(te => te.UserId)
            .IsUnique()
            .HasFilter("\"EndedAt\" IS NULL");

        builder.Property(te => te.StartedAt)
            .IsRequired();

        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_TimeEntries_EndAfterStart",
                "\"EndedAt\" IS NULL OR \"EndedAt\" > \"StartedAt\""));
    }
}