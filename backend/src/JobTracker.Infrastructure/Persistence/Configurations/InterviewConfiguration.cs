using JobTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Infrastructure.Persistence.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InterviewDate)
            .IsRequired();

        builder.Property(i => i.InterviewType)
            .IsRequired();

        builder.Property(i => i.Stage)
            .HasMaxLength(100);

        builder.Property(i => i.Interviewer)
            .HasMaxLength(200);

        builder.Property(i => i.Outcome)
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(i => i.Application)
            .WithMany(a => a.Interviews)
            .HasForeignKey(i => i.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft delete filter
        builder.HasQueryFilter(i => !i.IsDeleted);

        // Index for performance
        builder.HasIndex(i => i.InterviewDate);
        builder.HasIndex(i => i.ApplicationId);
    }
}