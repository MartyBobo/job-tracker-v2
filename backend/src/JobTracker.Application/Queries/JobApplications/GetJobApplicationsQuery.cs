using JobTracker.Application.DTOs.JobApplications;
using JobTracker.Domain.Enums;
using MediatR;
using Shared.Results;

namespace JobTracker.Application.Queries.JobApplications;

public record GetJobApplicationsQuery : IRequest<Result<List<JobApplicationDto>>>
{
    public Guid UserId { get; init; }
    public ApplicationStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool? IsRemote { get; init; }
    public string? SortBy { get; init; } = "AppliedDate";
    public bool SortDescending { get; init; } = true;
}