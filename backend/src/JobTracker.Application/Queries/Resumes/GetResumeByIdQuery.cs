using JobTracker.Application.Commands.Resumes;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Queries.Resumes;

public record GetResumeByIdQuery : IRequest<Result<ResumeDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}

public class GetResumeByIdQueryHandler : IRequestHandler<GetResumeByIdQuery, Result<ResumeDto>>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IFileStorageService _fileStorageService;

    public GetResumeByIdQueryHandler(
        IResumeRepository resumeRepository,
        IFileStorageService fileStorageService)
    {
        _resumeRepository = resumeRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<ResumeDto>> Handle(GetResumeByIdQuery request, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (resume == null)
        {
            return Result.Failure<ResumeDto>(new Error(
                ErrorCodes.NotFound,
                "Resume not found"));
        }

        // Verify ownership
        if (resume.UserId != request.UserId)
        {
            return Result.Failure<ResumeDto>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to view this resume"));
        }

        var applicationDetails = resume.Application != null
            ? $"{resume.Application.JobTitle} at {resume.Application.CompanyName}"
            : null;

        var fileUrl = !string.IsNullOrEmpty(resume.FilePath)
            ? _fileStorageService.GetFileUrl(resume.FilePath)
            : null;

        return Result.Success(new ResumeDto(
            resume.Id,
            resume.Name,
            resume.Description,
            resume.TemplateId,
            resume.Template.Name,
            resume.ApplicationId,
            applicationDetails,
            fileUrl,
            resume.FileFormat,
            resume.GeneratedAt,
            resume.Version,
            resume.CreatedAt
        ));
    }
}