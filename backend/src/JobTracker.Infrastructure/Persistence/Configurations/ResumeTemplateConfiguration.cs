using JobTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Infrastructure.Persistence.Configurations;

public class ResumeTemplateConfiguration : IEntityTypeConfiguration<ResumeTemplate>
{
    public void Configure(EntityTypeBuilder<ResumeTemplate> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rt => rt.Description)
            .HasMaxLength(500);

        builder.Property(rt => rt.TemplateData)
            .IsRequired();

        // Relationships
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.ResumeTemplates)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rt => rt.Resumes)
            .WithOne(r => r.Template)
            .HasForeignKey(r => r.TemplateId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete template if resumes exist

        // Soft delete filter
        builder.HasQueryFilter(rt => !rt.IsDeleted);

        // Index for performance
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => new { rt.UserId, rt.Name }).IsUnique();
    }
}