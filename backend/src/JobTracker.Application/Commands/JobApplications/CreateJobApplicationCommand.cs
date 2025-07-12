using JobTracker.Application.DTOs.JobApplications;
using JobTracker.Domain.Enums;
using MediatR;
using Shared.Results;

namespace JobTracker.Application.Commands.JobApplications;

public record CreateJobApplicationCommand : IRequest<Result<JobApplicationDto>>
{
    public Guid UserId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public bool IsRemote { get; init; }
    public bool SelfSourced { get; init; }
    public DateTime AppliedDate { get; init; }
    public ApplicationStatus Status { get; init; } = ApplicationStatus.Applied;
    public string? Notes { get; init; }
}