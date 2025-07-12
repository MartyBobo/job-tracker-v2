using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobTracker.Domain.Entities;

namespace JobTracker.Infrastructure.Configurations;

public class ResumeTemplateConfiguration : IEntityTypeConfiguration<ResumeTemplate>
{
    public void Configure(EntityTypeBuilder<ResumeTemplate> builder)
    {
        builder.ToTable("ResumeTemplates");
        
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(rt => rt.Description)
            .HasMaxLength(500);
            
        builder.Property(rt => rt.TemplateData)
            .IsRequired()
            .HasColumnType("TEXT"); // JSON data
            
        // Index
        builder.HasIndex(rt => rt.UserId);
        
        // Global query filter for soft delete
        builder.HasQueryFilter(rt => !rt.IsDeleted);
        
        // Relationships
        builder.HasMany(rt => rt.Resumes)
            .WithOne(r => r.Template)
            .HasForeignKey(r => r.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}