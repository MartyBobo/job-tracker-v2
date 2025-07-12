using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobTracker.Domain.Entities;
using JobTracker.Domain.Enums;

namespace JobTracker.Infrastructure.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");
        
        builder.HasKey(j => j.Id);
        
        builder.Property(j => j.JobTitle)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(j => j.CompanyName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(j => j.ContactEmail)
            .HasMaxLength(256);
            
        builder.Property(j => j.ContactPhone)
            .HasMaxLength(50);
            
        builder.Property(j => j.Notes)
            .HasMaxLength(2000);
            
        builder.Property(j => j.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(j => j.AppliedDate)
            .HasColumnType("TEXT"); // SQLite stores dates as TEXT
            
        // Indexes
        builder.HasIndex(j => j.UserId);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.AppliedDate);
        
        // Global query filter for soft delete
        builder.HasQueryFilter(j => !j.IsDeleted);
        
        // Relationships
        builder.HasMany(j => j.Interviews)
            .WithOne(i => i.Application)
            .HasForeignKey(i => i.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(j => j.Uploads)
            .WithOne(u => u.Application)
            .HasForeignKey(u => u.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}