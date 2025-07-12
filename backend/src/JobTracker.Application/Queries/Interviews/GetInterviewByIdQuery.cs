using JobTracker.Application.Commands.Interviews;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Queries.Interviews;

public record GetInterviewByIdQuery : IRequest<Result<InterviewDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}

public class GetInterviewByIdQueryHandler : IRequestHandler<GetInterviewByIdQuery, Result<InterviewDto>>
{
    private readonly IInterviewRepository _interviewRepository;

    public GetInterviewByIdQueryHandler(IInterviewRepository interviewRepository)
    {
        _interviewRepository = interviewRepository;
    }

    public async Task<Result<InterviewDto>> Handle(GetInterviewByIdQuery request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (interview == null)
        {
            return Result.Failure<InterviewDto>(new Error(
                ErrorCodes.InterviewNotFound,
                "Interview not found"));
        }

        // Verify the interview belongs to the user
        if (interview.Application.UserId != request.UserId)
        {
            return Result.Failure<InterviewDto>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to view this interview"));
        }

        return Result.Success(new InterviewDto(
            interview.Id,
            interview.ApplicationId,
            interview.InterviewDate,
            interview.InterviewType,
            interview.Stage,
            interview.Interviewer,
            interview.Outcome,
            interview.Notes,
            interview.CreatedAt,
            interview.Application?.JobTitle,
            interview.Application?.CompanyName
        ));
    }
}