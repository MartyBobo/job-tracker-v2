using JobTracker.Domain.Enums;

namespace JobTracker.Domain.Entities;

public class JobApplication : BaseEntity
{
    public Guid UserId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsRemote { get; set; }
    public bool SelfSourced { get; set; }
    public DateTime AppliedDate { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();
}