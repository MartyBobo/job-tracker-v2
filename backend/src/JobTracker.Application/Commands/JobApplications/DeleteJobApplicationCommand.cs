using MediatR;
using Shared.Results;

namespace JobTracker.Application.Commands.JobApplications;

public record DeleteJobApplicationCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}