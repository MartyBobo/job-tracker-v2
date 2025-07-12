using JobTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Infrastructure.Persistence.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.ResumeData)
            .IsRequired();

        builder.Property(r => r.FilePath)
            .HasMaxLength(500);

        builder.Property(r => r.FileFormat)
            .HasMaxLength(10);

        builder.Property(r => r.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.Resumes)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Template)
            .WithMany(t => t.Resumes)
            .HasForeignKey(r => r.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Application)
            .WithMany()
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Soft delete filter
        builder.HasQueryFilter(r => !r.IsDeleted);

        // Indexes for performance
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.TemplateId);
        builder.HasIndex(r => r.ApplicationId);
        builder.HasIndex(r => new { r.UserId, r.Name, r.Version });
    }
}