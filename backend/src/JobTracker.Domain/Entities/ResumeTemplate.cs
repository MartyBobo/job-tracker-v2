namespace JobTracker.Domain.Entities;

public class ResumeTemplate : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateData { get; set; } = string.Empty; // JSON data
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}