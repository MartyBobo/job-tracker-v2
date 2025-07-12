using JobTracker.Application.Commands.Interviews;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Results;

namespace JobTracker.Application.Queries.Interviews;

public record GetUpcomingInterviewsQuery : IRequest<Result<List<InterviewDto>>>
{
    public Guid UserId { get; init; }
    public int DaysAhead { get; init; } = 30; // Default to next 30 days
}

public class GetUpcomingInterviewsQueryHandler : IRequestHandler<GetUpcomingInterviewsQuery, Result<List<InterviewDto>>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public GetUpcomingInterviewsQueryHandler(
        IInterviewRepository interviewRepository,
        IJobApplicationRepository jobApplicationRepository)
    {
        _interviewRepository = interviewRepository;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<Result<List<InterviewDto>>> Handle(GetUpcomingInterviewsQuery request, CancellationToken cancellationToken)
    {
        // Get all user's applications
        var applications = await _jobApplicationRepository.GetByUserIdAsync(
            request.UserId,
            cancellationToken: cancellationToken);

        var now = DateTime.UtcNow;
        var endDate = now.AddDays(request.DaysAhead);

        var upcomingInterviews = new List<InterviewDto>();

        // For each application, get its interviews
        foreach (var app in applications)
        {
            var interviews = await _interviewRepository.GetByApplicationIdAsync(app.Id, cancellationToken);
            
            var appUpcomingInterviews = interviews
                .Where(i => i.InterviewDate >= now && 
                           i.InterviewDate <= endDate &&
                           i.Outcome != Domain.Enums.InterviewOutcome.Cancelled)
                .Select(i => new InterviewDto(
                    i.Id,
                    i.ApplicationId,
                    i.InterviewDate,
                    i.InterviewType,
                    i.Stage,
                    i.Interviewer,
                    i.Outcome,
                    i.Notes,
                    i.CreatedAt,
                    app.JobTitle,
                    app.CompanyName
                ));
            
            upcomingInterviews.AddRange(appUpcomingInterviews);
        }

        return Result.Success(upcomingInterviews.OrderBy(i => i.InterviewDate).ToList());
    }
}