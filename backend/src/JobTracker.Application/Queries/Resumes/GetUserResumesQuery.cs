using JobTracker.Application.Commands.Resumes;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Results;

namespace JobTracker.Application.Queries.Resumes;

public record GetUserResumesQuery : IRequest<Result<List<ResumeDto>>>
{
    public Guid UserId { get; init; }
    public Guid? TemplateId { get; init; }
    public Guid? ApplicationId { get; init; }
}

public class GetUserResumesQueryHandler : IRequestHandler<GetUserResumesQuery, Result<List<ResumeDto>>>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IFileStorageService _fileStorageService;

    public GetUserResumesQueryHandler(
        IResumeRepository resumeRepository,
        IFileStorageService fileStorageService)
    {
        _resumeRepository = resumeRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<List<ResumeDto>>> Handle(GetUserResumesQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Entities.Resume> resumes;

        if (request.ApplicationId.HasValue)
        {
            resumes = await _resumeRepository.GetByApplicationIdAsync(request.ApplicationId.Value, cancellationToken);
            // Filter by user for security
            resumes = resumes.Where(r => r.UserId == request.UserId).ToList();
        }
        else if (request.TemplateId.HasValue)
        {
            resumes = await _resumeRepository.GetByTemplateIdAsync(request.TemplateId.Value, cancellationToken);
            // Filter by user for security
            resumes = resumes.Where(r => r.UserId == request.UserId).ToList();
        }
        else
        {
            resumes = await _resumeRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        }

        var resumeDtos = resumes.Select(resume =>
        {
            var applicationDetails = resume.Application != null
                ? $"{resume.Application.JobTitle} at {resume.Application.CompanyName}"
                : null;

            var fileUrl = !string.IsNullOrEmpty(resume.FilePath)
                ? _fileStorageService.GetFileUrl(resume.FilePath)
                : null;

            return new ResumeDto(
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
            );
        }).ToList();

        return Result.Success(resumeDtos);
    }
}