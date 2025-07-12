using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.Resumes;

public record DeleteResumeCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}

public class DeleteResumeCommandHandler : IRequestHandler<DeleteResumeCommand, Result<Unit>>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteResumeCommandHandler> _logger;

    public DeleteResumeCommandHandler(
        IResumeRepository resumeRepository,
        IFileStorageService fileStorageService,
        ILogger<DeleteResumeCommandHandler> logger)
    {
        _resumeRepository = resumeRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteResumeCommand request, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (resume == null)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.NotFound,
                "Resume not found"));
        }

        // Verify ownership
        if (resume.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to delete this resume"));
        }

        // Delete the file if it exists
        if (!string.IsNullOrEmpty(resume.FilePath))
        {
            await _fileStorageService.DeleteAsync(resume.FilePath, cancellationToken);
        }

        // Soft delete the resume record
        await _resumeRepository.DeleteAsync(resume, cancellationToken);

        _logger.LogInformation(
            "Resume deleted successfully. Id: {ResumeId}, Name: {ResumeName}",
            resume.Id, resume.Name);

        return Result.Success(Unit.Value);
    }
}