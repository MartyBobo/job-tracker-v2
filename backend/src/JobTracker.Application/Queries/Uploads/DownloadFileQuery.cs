using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Queries.Uploads;

public record DownloadFileQuery : IRequest<Result<DownloadFileResult>>
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
}

public record DownloadFileResult(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize
);

public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, Result<DownloadFileResult>>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IUploadRepository _uploadRepository;
    private readonly ILogger<DownloadFileQueryHandler> _logger;

    public DownloadFileQueryHandler(
        IFileStorageService fileStorageService,
        IUploadRepository uploadRepository,
        ILogger<DownloadFileQueryHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _uploadRepository = uploadRepository;
        _logger = logger;
    }

    public async Task<Result<DownloadFileResult>> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        // Get upload record
        var upload = await _uploadRepository.GetByIdAsync(request.FileId, cancellationToken);
        
        if (upload == null)
        {
            return Result.Failure<DownloadFileResult>(new Error(
                ErrorCodes.FileNotFound,
                "File not found"));
        }

        // Verify user owns the file
        if (upload.UserId != request.UserId)
        {
            return Result.Failure<DownloadFileResult>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to access this file"));
        }

        try
        {
            // Download file from storage
            var downloadResult = await _fileStorageService.DownloadAsync(upload.FilePath, cancellationToken);
            
            if (downloadResult == null)
            {
                _logger.LogWarning("File not found in storage: {FilePath}", upload.FilePath);
                return Result.Failure<DownloadFileResult>(new Error(
                    ErrorCodes.FileNotFound,
                    "File not found in storage"));
            }

            return Result.Success(new DownloadFileResult(
                downloadResult.FileStream,
                upload.FileName,
                upload.ContentType,
                downloadResult.FileSize
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileId}", request.FileId);
            return Result.Failure<DownloadFileResult>(new Error(
                ErrorCodes.InternalServerError,
                "An error occurred while downloading the file"));
        }
    }
}