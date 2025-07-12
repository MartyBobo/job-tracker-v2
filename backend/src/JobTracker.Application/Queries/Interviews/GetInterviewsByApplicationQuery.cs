using JobTracker.Application.Commands.Interviews;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Queries.Interviews;

public record GetInterviewsByApplicationQuery : IRequest<Result<List<InterviewDto>>>
{
    public Guid ApplicationId { get; init; }
    public Guid UserId { get; init; }
}

public class GetInterviewsByApplicationQueryHandler : IRequestHandler<GetInterviewsByApplicationQuery, Result<List<InterviewDto>>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GetInterviewsByApplicationQueryHandler(
        IInterviewRepository interviewRepository,
        IJobApplicationRepository jobApplicationRepository)
    {
        _interviewRepository = interviewRepository;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<Result<List<InterviewDto>>> Handle(GetInterviewsByApplicationQuery request, CancellationToken cancellationToken)
    {
        // Verify the application belongs to the user
        var application = await _jobApplicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
        
        if (application == null || application.UserId != request.UserId)
        {
            return Result.Failure<List<InterviewDto>>(new Error(
                ErrorCodes.JobApplicationNotFound,
                "Job application not found"));
        }

        var interviews = await _interviewRepository.GetByApplicationIdAsync(request.ApplicationId, cancellationToken);

        var interviewDtos = interviews.Select(i => new InterviewDto(
            i.Id,
            i.ApplicationId,
            i.InterviewDate,
            i.InterviewType,
            i.Stage,
            i.Interviewer,
            i.Outcome,
            i.Notes,
            i.CreatedAt,
            application.JobTitle,
            application.CompanyName
        )).ToList();

        return Result.Success(interviewDtos);
    }
}