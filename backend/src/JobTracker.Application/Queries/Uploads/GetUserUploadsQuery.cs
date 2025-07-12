using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Domain.Enums;
using MediatR;
using Shared.Results;

namespace JobTracker.Application.Queries.Uploads;

public record GetUserUploadsQuery : IRequest<Result<List<UploadDto>>>
{
    public Guid UserId { get; init; }
    public Guid? ApplicationId { get; init; }
    public DocumentType? DocumentType { get; init; }
}

public record UploadDto(
    Guid Id,
    string FileName,
    long FileSize,
    string ContentType,
    DocumentType DocumentType,
    string? Description,
    Guid? ApplicationId,
    DateTime UploadedAt,
    string FileUrl
);

public class GetUserUploadsQueryHandler : IRequestHandler<GetUserUploadsQuery, Result<List<UploadDto>>>
{
    private readonly IUploadRepository _uploadRepository;
    private readonly IFileStorageService _fileStorageService;

    public GetUserUploadsQueryHandler(
        IUploadRepository uploadRepository,
        IFileStorageService fileStorageService)
    {
        _uploadRepository = uploadRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<List<UploadDto>>> Handle(GetUserUploadsQuery request, CancellationToken cancellationToken)
    {
        List<Upload> uploads;

        if (request.ApplicationId.HasValue)
        {
            uploads = await _uploadRepository.GetByApplicationIdAsync(request.ApplicationId.Value, cancellationToken);
            // Filter by user to ensure security
            uploads = uploads.Where(u => u.UserId == request.UserId).ToList();
        }
        else
        {
            uploads = await _uploadRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        }

        // Filter by document type if specified
        if (request.DocumentType.HasValue)
        {
            uploads = uploads.Where(u => u.DocumentType == request.DocumentType.Value).ToList();
        }

        var uploadDtos = uploads.Select(u => new UploadDto(
            u.Id,
            u.FileName,
            u.FileSize,
            u.ContentType,
            u.DocumentType,
            u.Description,
            u.ApplicationId,
            u.CreatedAt,
            _fileStorageService.GetFileUrl(u.FilePath)
        )).ToList();

        return Result.Success(uploadDtos);
    }
}