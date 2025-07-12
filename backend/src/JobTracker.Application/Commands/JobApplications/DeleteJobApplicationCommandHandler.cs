using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.JobApplications;

public class DeleteJobApplicationCommandHandler : IRequestHandler<DeleteJobApplicationCommand, Result>
{
    private readonly IJobApplicationRepository _repository;
    private readonly ILogger<DeleteJobApplicationCommandHandler> _logger;

    public DeleteJobApplicationCommandHandler(
        IJobApplicationRepository repository,
        ILogger<DeleteJobApplicationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteJobApplicationCommand request,
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
                "You are not authorized to delete this job application"));
        }

        try
        {
            await _repository.DeleteAsync(application, cancellationToken);
            
            _logger.LogInformation("Deleted job application {Id}", request.Id);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job application {Id}", request.Id);
            return Result.Failure(new Error(
                ErrorCodes.JobApplicationDeleteFailed,
                "Failed to delete job application"));
        }
    }
}