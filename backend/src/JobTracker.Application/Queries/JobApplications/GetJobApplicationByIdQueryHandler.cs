using JobTracker.Application.DTOs.Interviews;
using JobTracker.Application.DTOs.JobApplications;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Queries.JobApplications;

public class GetJobApplicationByIdQueryHandler : IRequestHandler<GetJobApplicationByIdQuery, Result<JobApplicationDto>>
{
    private readonly IJobApplicationRepository _repository;

    public GetJobApplicationByIdQueryHandler(IJobApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JobApplicationDto>> Handle(
        GetJobApplicationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var application = request.IncludeInterviews
            ? await _repository.GetByIdWithInterviewsAsync(request.Id, cancellationToken)
            : await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (application == null)
        {
            return Result.Failure<JobApplicationDto>(new Error(
                ErrorCodes.JobApplicationNotFound,
                $"Job application with ID {request.Id} not found"));
        }

        if (application.UserId != request.UserId)
        {
            return Result.Failure<JobApplicationDto>(new Error(
                ErrorCodes.Forbidden,
                "You are not authorized to view this job application"));
        }

        var dto = new JobApplicationDto
        {
            Id = application.Id,
            JobTitle = application.JobTitle,
            CompanyName = application.CompanyName,
            ContactEmail = application.ContactEmail,
            ContactPhone = application.ContactPhone,
            IsRemote = application.IsRemote,
            SelfSourced = application.SelfSourced,
            AppliedDate = application.AppliedDate,
            Status = application.Status,
            Notes = application.Notes,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
            InterviewCount = application.Interviews.Count,
            UploadCount = application.Uploads.Count
        };

        if (request.IncludeInterviews && application.Interviews.Any())
        {
            dto.Interviews = application.Interviews
                .OrderBy(i => i.InterviewDate)
                .Select(i => new InterviewDto
                {
                    Id = i.Id,
                    ApplicationId = i.ApplicationId,
                    InterviewDate = i.InterviewDate,
                    InterviewType = i.InterviewType,
                    Stage = i.Stage,
                    Interviewer = i.Interviewer,
                    Outcome = i.Outcome,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt
                })
                .ToList();
        }

        return Result.Success(dto);
    }
}