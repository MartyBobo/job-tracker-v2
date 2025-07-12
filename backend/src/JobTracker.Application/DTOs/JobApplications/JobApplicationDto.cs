using JobTracker.Application.DTOs.Interviews;
using JobTracker.Domain.Enums;

namespace JobTracker.Application.DTOs.JobApplications;

public class JobApplicationDto
{
    public Guid Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsRemote { get; set; }
    public bool SelfSourced { get; set; }
    public DateTime AppliedDate { get; set; }
    public ApplicationStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public List<InterviewDto>? Interviews { get; set; }
    public int? InterviewCount { get; set; }
    public int? UploadCount { get; set; }
}