using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using JobTracker.Domain.Entities;

namespace JobTracker.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.PasswordHash)
            .IsRequired();
            
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);
            
        // Global query filter for soft delete
        builder.HasQueryFilter(u => !u.IsDeleted);
            
        // Relationships
        builder.HasMany(u => u.JobApplications)
            .WithOne(j => j.User)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.Uploads)
            .WithOne(up => up.User)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.ResumeTemplates)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}