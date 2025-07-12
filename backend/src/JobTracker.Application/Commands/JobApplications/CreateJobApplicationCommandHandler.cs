using JobTracker.Application.DTOs.JobApplications;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.JobApplications;

public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, Result<JobApplicationDto>>
{
    private readonly IJobApplicationRepository _repository;
    private readonly ILogger<CreateJobApplicationCommandHandler> _logger;

    public CreateJobApplicationCommandHandler(
        IJobApplicationRepository repository,
        ILogger<CreateJobApplicationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<JobApplicationDto>> Handle(
        CreateJobApplicationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var application = new JobApplication
            {
                UserId = request.UserId,
                JobTitle = request.JobTitle,
                CompanyName = request.CompanyName,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                IsRemote = request.IsRemote,
                SelfSourced = request.SelfSourced,
                AppliedDate = request.AppliedDate,
                Status = request.Status,
                Notes = request.Notes
            };

            var created = await _repository.CreateAsync(application, cancellationToken);

            var dto = new JobApplicationDto
            {
                Id = created.Id,
                JobTitle = created.JobTitle,
                CompanyName = created.CompanyName,
                ContactEmail = created.ContactEmail,
                ContactPhone = created.ContactPhone,
                IsRemote = created.IsRemote,
                SelfSourced = created.SelfSourced,
                AppliedDate = created.AppliedDate,
                Status = created.Status,
                Notes = created.Notes,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                InterviewCount = 0,
                UploadCount = 0
            };

            _logger.LogInformation("Created job application {Id} for user {UserId}", created.Id, request.UserId);
            
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job application for user {UserId}", request.UserId);
            return Result.Failure<JobApplicationDto>(
                new Error(ErrorCodes.JobApplicationCreateFailed, "Failed to create job application"));
        }
    }
}