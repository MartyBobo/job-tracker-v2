using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobTracker.Domain.Entities;

namespace JobTracker.Infrastructure.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("Interviews");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.InterviewDate)
            .HasColumnType("TEXT")
            .IsRequired();
            
        builder.Property(i => i.InterviewType)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(i => i.Stage)
            .HasMaxLength(100);
            
        builder.Property(i => i.Interviewer)
            .HasMaxLength(200);
            
        builder.Property(i => i.Outcome)
            .HasMaxLength(100);
            
        builder.Property(i => i.Notes)
            .HasMaxLength(2000);
            
        // Index
        builder.HasIndex(i => i.ApplicationId);
        builder.HasIndex(i => i.InterviewDate);
        
        // Global query filter for soft delete
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}