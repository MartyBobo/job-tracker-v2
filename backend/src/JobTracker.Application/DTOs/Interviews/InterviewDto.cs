using JobTracker.Domain.Enums;

namespace JobTracker.Application.DTOs.Interviews;

public class InterviewDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public DateTime InterviewDate { get; set; }
    public InterviewType InterviewType { get; set; }
    public string? Stage { get; set; }
    public string? Interviewer { get; set; }
    public InterviewOutcome? Outcome { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}