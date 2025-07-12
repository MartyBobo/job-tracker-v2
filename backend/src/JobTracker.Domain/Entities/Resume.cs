namespace JobTracker.Domain.Entities;

public class Resume : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ResumeData { get; set; } = string.Empty; // JSON data with filled values
    public string? FilePath { get; set; } // Path to generated PDF/DOCX
    public string? FileFormat { get; set; } // PDF, DOCX, etc.
    public DateTime? GeneratedAt { get; set; }
    public int Version { get; set; } = 1;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ResumeTemplate Template { get; set; } = null!;
    public virtual JobApplication? Application { get; set; }
}