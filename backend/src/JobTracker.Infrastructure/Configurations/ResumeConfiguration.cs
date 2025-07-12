using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobTracker.Domain.Entities;

namespace JobTracker.Infrastructure.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("Resumes");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.FilePath)
            .IsRequired()
            .HasMaxLength(1000);
            
        // Index
        builder.HasIndex(r => r.TemplateId);
        
        // Global query filter for soft delete
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}