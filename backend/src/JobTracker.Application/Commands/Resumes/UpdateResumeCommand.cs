using FluentValidation;
using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.Resumes;

public record UpdateResumeCommand : IRequest<Result<ResumeDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class UpdateResumeCommandValidator : AbstractValidator<UpdateResumeCommand>
{
    public UpdateResumeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateResumeCommandHandler : IRequestHandler<UpdateResumeCommand, Result<ResumeDto>>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly ILogger<UpdateResumeCommandHandler> _logger;

    public UpdateResumeCommandHandler(
        IResumeRepository resumeRepository,
        ILogger<UpdateResumeCommandHandler> logger)
    {
        _resumeRepository = resumeRepository;
        _logger = logger;
    }

    public async Task<Result<ResumeDto>> Handle(UpdateResumeCommand request, CancellationToken cancellationToken)
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
                "You don't have permission to update this resume"));
        }

        // Update only metadata, not the generated content
        resume.Name = request.Name;
        resume.Description = request.Description;

        await _resumeRepository.UpdateAsync(resume, cancellationToken);

        _logger.LogInformation(
            "Resume updated successfully. Id: {ResumeId}, Name: {ResumeName}",
            resume.Id, resume.Name);

        var applicationDetails = resume.Application != null
            ? $"{resume.Application.JobTitle} at {resume.Application.CompanyName}"
            : null;

        return Result.Success(new ResumeDto(
            resume.Id,
            resume.Name,
            resume.Description,
            resume.TemplateId,
            resume.Template.Name,
            resume.ApplicationId,
            applicationDetails,
            resume.FilePath,
            resume.FileFormat,
            resume.GeneratedAt,
            resume.Version,
            resume.CreatedAt
        ));
    }
}