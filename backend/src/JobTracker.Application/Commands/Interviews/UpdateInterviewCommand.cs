using FluentValidation;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.Interviews;

public record UpdateInterviewCommand : IRequest<Result<InterviewDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateTime InterviewDate { get; init; }
    public InterviewType InterviewType { get; init; }
    public string? Stage { get; init; }
    public string? Interviewer { get; init; }
    public InterviewOutcome? Outcome { get; init; }
    public string? Notes { get; init; }
}

public class UpdateInterviewCommandValidator : AbstractValidator<UpdateInterviewCommand>
{
    public UpdateInterviewCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.InterviewDate).NotEmpty();
        RuleFor(x => x.InterviewType).IsInEnum();
        RuleFor(x => x.Stage).MaximumLength(100);
        RuleFor(x => x.Interviewer).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        When(x => x.Outcome.HasValue, () =>
        {
            RuleFor(x => x.Outcome!.Value).IsInEnum();
        });
    }
}

public class UpdateInterviewCommandHandler : IRequestHandler<UpdateInterviewCommand, Result<InterviewDto>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ILogger<UpdateInterviewCommandHandler> _logger;

    public UpdateInterviewCommandHandler(
        IInterviewRepository interviewRepository,
        IJobApplicationRepository jobApplicationRepository,
        ILogger<UpdateInterviewCommandHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _logger = logger;
    }

    public async Task<Result<InterviewDto>> Handle(UpdateInterviewCommand request, CancellationToken cancellationToken)
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
                "You don't have permission to update this interview"));
        }

        // Check for scheduling conflicts if date changed
        if (interview.InterviewDate != request.InterviewDate)
        {
            var hasConflict = await _interviewRepository.HasScheduleConflictAsync(
                request.UserId,
                request.InterviewDate,
                TimeSpan.FromHours(1),
                excludeInterviewId: request.Id,
                cancellationToken: cancellationToken);

            if (hasConflict)
            {
                _logger.LogWarning(
                    "User {UserId} is rescheduling interview {InterviewId} to a time that may conflict: {InterviewDate}",
                    request.UserId, request.Id, request.InterviewDate);
            }
        }

        // Update interview
        interview.InterviewDate = request.InterviewDate;
        interview.InterviewType = request.InterviewType;
        interview.Stage = request.Stage;
        interview.Interviewer = request.Interviewer;
        interview.Outcome = request.Outcome;
        interview.Notes = request.Notes;

        await _interviewRepository.UpdateAsync(interview, cancellationToken);

        // Update application status based on outcome
        if (request.Outcome.HasValue && interview.Application != null)
        {
            var application = interview.Application;
            
            switch (request.Outcome.Value)
            {
                case InterviewOutcome.Passed:
                    // Check if there are more interviews scheduled
                    var futureInterviews = await _interviewRepository.GetByApplicationIdAsync(
                        application.Id, cancellationToken);
                    var hasFutureInterviews = futureInterviews.Any(i => 
                        i.Id != interview.Id && 
                        i.InterviewDate > DateTime.UtcNow && 
                        i.Outcome != InterviewOutcome.Cancelled);
                    
                    if (!hasFutureInterviews)
                    {
                        application.Status = ApplicationStatus.Offer;
                    }
                    break;
                    
                case InterviewOutcome.Failed:
                    application.Status = ApplicationStatus.Declined;
                    break;
            }
            
            await _jobApplicationRepository.UpdateAsync(application, cancellationToken);
        }

        _logger.LogInformation(
            "Interview updated successfully. Id: {InterviewId}, Outcome: {Outcome}",
            interview.Id, interview.Outcome);

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