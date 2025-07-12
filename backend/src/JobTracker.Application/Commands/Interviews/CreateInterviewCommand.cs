using FluentValidation;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.Interviews;

public record CreateInterviewCommand : IRequest<Result<InterviewDto>>
{
    public Guid UserId { get; init; }
    public Guid ApplicationId { get; init; }
    public DateTime InterviewDate { get; init; }
    public InterviewType InterviewType { get; init; }
    public string? Stage { get; init; }
    public string? Interviewer { get; init; }
    public string? Notes { get; init; }
}

public record InterviewDto(
    Guid Id,
    Guid ApplicationId,
    DateTime InterviewDate,
    InterviewType InterviewType,
    string? Stage,
    string? Interviewer,
    InterviewOutcome? Outcome,
    string? Notes,
    DateTime CreatedAt,
    string? JobTitle,
    string? CompanyName
);

public class CreateInterviewCommandValidator : AbstractValidator<CreateInterviewCommand>
{
    public CreateInterviewCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.InterviewDate)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow.AddMinutes(-30))
            .WithMessage("Interview date must be in the future");
        RuleFor(x => x.InterviewType).IsInEnum();
        RuleFor(x => x.Stage).MaximumLength(100);
        RuleFor(x => x.Interviewer).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class CreateInterviewCommandHandler : IRequestHandler<CreateInterviewCommand, Result<InterviewDto>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ILogger<CreateInterviewCommandHandler> _logger;

    public CreateInterviewCommandHandler(
        IInterviewRepository interviewRepository,
        IJobApplicationRepository jobApplicationRepository,
        ILogger<CreateInterviewCommandHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _logger = logger;
    }

    public async Task<Result<InterviewDto>> Handle(CreateInterviewCommand request, CancellationToken cancellationToken)
    {
        // Verify the application exists and belongs to the user
        var application = await _jobApplicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
        
        if (application == null || application.UserId != request.UserId)
        {
            return Result.Failure<InterviewDto>(new Error(
                ErrorCodes.JobApplicationNotFound,
                "Job application not found"));
        }

        // Check for scheduling conflicts (optional - warn but don't block)
        var hasConflict = await _interviewRepository.HasScheduleConflictAsync(
            request.UserId,
            request.InterviewDate,
            TimeSpan.FromHours(1),
            cancellationToken: cancellationToken);

        if (hasConflict)
        {
            _logger.LogWarning(
                "User {UserId} is scheduling an interview that may conflict with another at {InterviewDate}",
                request.UserId, request.InterviewDate);
        }

        var interview = new Interview
        {
            ApplicationId = request.ApplicationId,
            InterviewDate = request.InterviewDate,
            InterviewType = request.InterviewType,
            Stage = request.Stage,
            Interviewer = request.Interviewer,
            Notes = request.Notes
        };

        await _interviewRepository.CreateAsync(interview, cancellationToken);

        // Update application status to "Interviewing" if it's still in "Applied" state
        if (application.Status == ApplicationStatus.Applied)
        {
            application.Status = ApplicationStatus.Interviewing;
            await _jobApplicationRepository.UpdateAsync(application, cancellationToken);
        }

        _logger.LogInformation(
            "Interview created successfully. Id: {InterviewId}, ApplicationId: {ApplicationId}, Date: {InterviewDate}",
            interview.Id, interview.ApplicationId, interview.InterviewDate);

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
            application.JobTitle,
            application.CompanyName
        ));
    }
}