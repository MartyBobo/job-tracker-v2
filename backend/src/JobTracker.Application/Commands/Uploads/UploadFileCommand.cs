using FluentValidation;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.Uploads;

public record UploadFileCommand : IRequest<Result<UploadFileResult>>
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public long FileSize { get; init; }
    public Guid UserId { get; init; }
    public Guid? ApplicationId { get; init; }
    public DocumentType DocumentType { get; init; }
    public string? Description { get; init; }
}

public record UploadFileResult(
    Guid Id,
    string FileName,
    string FilePath,
    string FileUrl,
    long FileSize,
    DocumentType DocumentType,
    DateTime UploadedAt
);

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileStream).NotNull();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.FileSize).GreaterThan(0);
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
    }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<UploadFileResult>>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IFileValidationService _fileValidationService;
    private readonly IUploadRepository _uploadRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ILogger<UploadFileCommandHandler> _logger;
    private const long MaxStoragePerUser = 100 * 1024 * 1024; // 100MB per user

    public UploadFileCommandHandler(
        IFileStorageService fileStorageService,
        IFileValidationService fileValidationService,
        IUploadRepository uploadRepository,
        IJobApplicationRepository jobApplicationRepository,
        ILogger<UploadFileCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _fileValidationService = fileValidationService;
        _uploadRepository = uploadRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _logger = logger;
    }

    public async Task<Result<UploadFileResult>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // Validate file
        var (isValid, errorMessage) = _fileValidationService.ValidateFile(
            request.FileName, 
            request.FileSize, 
            request.ContentType);

        if (!isValid)
        {
            return Result.Failure<UploadFileResult>(new Error(
                ErrorCodes.ValidationFailed,
                errorMessage ?? "File validation failed"));
        }

        // Check user storage quota
        var currentUsage = await _uploadRepository.GetTotalStorageUsedByUserAsync(request.UserId, cancellationToken);
        if (currentUsage + request.FileSize > MaxStoragePerUser)
        {
            return Result.Failure<UploadFileResult>(new Error(
                ErrorCodes.StorageQuotaExceeded,
                "Storage quota exceeded. Please delete some files before uploading new ones."));
        }

        // Verify application ownership if ApplicationId is provided
        if (request.ApplicationId.HasValue)
        {
            var application = await _jobApplicationRepository.GetByIdAsync(
                request.ApplicationId.Value, 
                cancellationToken);

            if (application == null || application.UserId != request.UserId)
            {
                return Result.Failure<UploadFileResult>(new Error(
                    ErrorCodes.JobApplicationNotFound,
                    "Job application not found"));
            }
        }

        try
        {
            // Upload file to storage
            var uploadResult = await _fileStorageService.UploadAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                request.UserId,
                cancellationToken);

            // Create upload entity
            var upload = new Upload
            {
                FileName = request.FileName,
                FilePath = uploadResult.FilePath,
                FileSize = uploadResult.FileSize,
                ContentType = request.ContentType,
                DocumentType = request.DocumentType,
                Description = request.Description,
                UserId = request.UserId,
                ApplicationId = request.ApplicationId
            };

            await _uploadRepository.CreateAsync(upload, cancellationToken);

            _logger.LogInformation(
                "File uploaded successfully. Id: {UploadId}, FileName: {FileName}, UserId: {UserId}",
                upload.Id, upload.FileName, upload.UserId);

            return Result.Success(new UploadFileResult(
                upload.Id,
                upload.FileName,
                upload.FilePath,
                uploadResult.FileUrl,
                upload.FileSize,
                upload.DocumentType,
                upload.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", request.FileName);
            return Result.Failure<UploadFileResult>(new Error(
                ErrorCodes.InternalServerError,
                "An error occurred while uploading the file"));
        }
    }
}