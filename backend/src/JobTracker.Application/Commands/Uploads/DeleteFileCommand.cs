using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.Uploads;

public record DeleteFileCommand : IRequest<Result<Unit>>
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
}

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result<Unit>>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IUploadRepository _uploadRepository;
    private readonly ILogger<DeleteFileCommandHandler> _logger;

    public DeleteFileCommandHandler(
        IFileStorageService fileStorageService,
        IUploadRepository uploadRepository,
        ILogger<DeleteFileCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _uploadRepository = uploadRepository;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        // Get upload record
        var upload = await _uploadRepository.GetByIdAsync(request.FileId, cancellationToken);
        
        if (upload == null)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.FileNotFound,
                "File not found"));
        }

        // Verify user owns the file
        if (upload.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to delete this file"));
        }

        try
        {
            // Delete file from storage
            var deleted = await _fileStorageService.DeleteAsync(upload.FilePath, cancellationToken);
            
            // Soft delete the upload record
            await _uploadRepository.DeleteAsync(upload, cancellationToken);

            _logger.LogInformation(
                "File deleted successfully. Id: {FileId}, FileName: {FileName}, UserId: {UserId}",
                upload.Id, upload.FileName, upload.UserId);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileId}", request.FileId);
            return Result.Failure<Unit>(new Error(
                ErrorCodes.InternalServerError,
                "An error occurred while deleting the file"));
        }
    }
}