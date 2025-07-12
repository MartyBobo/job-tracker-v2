using JobTracker.Application.DTOs.JobApplications;
using MediatR;
using Shared.Results;

namespace JobTracker.Application.Queries.JobApplications;

public record GetJobApplicationByIdQuery : IRequest<Result<JobApplicationDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public bool IncludeInterviews { get; init; } = false;
}