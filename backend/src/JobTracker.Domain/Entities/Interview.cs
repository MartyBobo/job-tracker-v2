using JobTracker.Domain.Enums;

namespace JobTracker.Domain.Entities;

public class Interview : BaseEntity
{
    public Guid ApplicationId { get; set; }
    public DateTime InterviewDate { get; set; }
    public InterviewType InterviewType { get; set; }
    public string? Stage { get; set; }
    public string? Interviewer { get; set; }
    public InterviewOutcome? Outcome { get; set; }
    public string? Notes { get; set; }
    
    // Navigation property
    public virtual JobApplication Application { get; set; } = null!;
}