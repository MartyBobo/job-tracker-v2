namespace JobTracker.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Navigation properties
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();
    public virtual ICollection<ResumeTemplate> ResumeTemplates { get; set; } = new List<ResumeTemplate>();
    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}