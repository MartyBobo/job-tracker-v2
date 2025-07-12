using JobTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Infrastructure.Persistence.Configurations;

public class UploadConfiguration : IEntityTypeConfiguration<Upload>
{
    public void Configure(EntityTypeBuilder<Upload> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Description)
            .HasMaxLength(500);

        builder.Property(u => u.FileSize)
            .IsRequired();

        builder.Property(u => u.DocumentType)
            .IsRequired();

        // Relationships
        builder.HasOne(u => u.User)
            .WithMany(user => user.Uploads)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Application)
            .WithMany(app => app.Uploads)
            .HasForeignKey(u => u.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Soft delete filter
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}