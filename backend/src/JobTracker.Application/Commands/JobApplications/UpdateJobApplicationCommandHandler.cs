using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.JobApplications;

public class UpdateJobApplicationCommandHandler : IRequestHandler<UpdateJobApplicationCommand, Result>
{
    private readonly IJobApplicationRepository _repository;
    private readonly ILogger<UpdateJobApplicationCommandHandler> _logger;

    public UpdateJobApplicationCommandHandler(
        IJobApplicationRepository repository,
        ILogger<UpdateJobApplicationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateJobApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (application == null)
        {
            return Result.Failure(new Error(
                ErrorCodes.JobApplicationNotFound,
                $"Job application with ID {request.Id} not found"));
        }

        if (application.UserId != request.UserId)
        {
            return Result.Failure(new Error(
                ErrorCodes.Forbidden,
                "You are not authorized to update this job application"));
        }

        application.JobTitle = request.JobTitle;
        application.CompanyName = request.CompanyName;
        application.ContactEmail = request.ContactEmail;
        application.ContactPhone = request.ContactPhone;
        application.IsRemote = request.IsRemote;
        application.SelfSourced = request.SelfSourced;
        application.AppliedDate = request.AppliedDate;
        application.Status = request.Status;
        application.Notes = request.Notes;

        try
        {
            await _repository.UpdateAsync(application, cancellationToken);
            
            _logger.LogInformation("Updated job application {Id}", request.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job application {Id}", request.Id);
            return Result.Failure(new Error(
                ErrorCodes.JobApplicationUpdateFailed,
                "Failed to update job application"));
        }
    }
}