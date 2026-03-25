using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimePlanner.Domain.Entities;

namespace TimePlanner.Repositories.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Category)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Assignments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(a => a.AssignmentName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Description)
                .HasMaxLength(256);

            builder.Property(a => a.Type)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.EstimatedHours)
                .HasDefaultValue(0);

            builder.Property(a => a.SpentHours)
                .HasDefaultValue(0);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("NOW()");
        }
    }
}
