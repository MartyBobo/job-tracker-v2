using FluentValidation;
using JobTracker.Application.Commands.ResumeTemplates;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;
using System.Text.Json;

namespace JobTracker.Application.Commands.Resumes;

public record GenerateResumeCommand : IRequest<Result<ResumeDto>>
{
    public Guid UserId { get; init; }
    public Guid TemplateId { get; init; }
    public Guid? ApplicationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ResumeTemplateData? CustomData { get; init; } // Optional overrides for template data
    public string Format { get; init; } = "pdf"; // pdf, docx, html
}

public record ResumeDto(
    Guid Id,
    string Name,
    string? Description,
    Guid TemplateId,
    string TemplateName,
    Guid? ApplicationId,
    string? ApplicationDetails,
    string? FilePath,
    string? FileFormat,
    DateTime? GeneratedAt,
    int Version,
    DateTime CreatedAt
);

public class GenerateResumeCommandValidator : AbstractValidator<GenerateResumeCommand>
{
    public GenerateResumeCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Format)
            .NotEmpty()
            .Must(f => new[] { "pdf", "docx", "html" }.Contains(f.ToLower()))
            .WithMessage("Format must be pdf, docx, or html");
    }
}

public class GenerateResumeCommandHandler : IRequestHandler<GenerateResumeCommand, Result<ResumeDto>>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IResumeTemplateRepository _templateRepository;
    private readonly IJobApplicationRepository _applicationRepository;
    private readonly IResumeGenerationService _generationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<GenerateResumeCommandHandler> _logger;

    public GenerateResumeCommandHandler(
        IResumeRepository resumeRepository,
        IResumeTemplateRepository templateRepository,
        IJobApplicationRepository applicationRepository,
        IResumeGenerationService generationService,
        IFileStorageService fileStorageService,
        ILogger<GenerateResumeCommandHandler> logger)
    {
        _resumeRepository = resumeRepository;
        _templateRepository = templateRepository;
        _applicationRepository = applicationRepository;
        _generationService = generationService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<ResumeDto>> Handle(GenerateResumeCommand request, CancellationToken cancellationToken)
    {
        // Get template
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template == null || template.UserId != request.UserId)
        {
            return Result.Failure<ResumeDto>(new Error(
                ErrorCodes.NotFound,
                "Resume template not found"));
        }

        // Verify application ownership if provided
        string? applicationDetails = null;
        if (request.ApplicationId.HasValue)
        {
            var application = await _applicationRepository.GetByIdAsync(request.ApplicationId.Value, cancellationToken);
            if (application == null || application.UserId != request.UserId)
            {
                return Result.Failure<ResumeDto>(new Error(
                    ErrorCodes.JobApplicationNotFound,
                    "Job application not found"));
            }
            applicationDetails = $"{application.JobTitle} at {application.CompanyName}";
        }

        // Merge template data with custom data if provided
        var templateData = JsonSerializer.Deserialize<ResumeTemplateData>(template.TemplateData);
        if (request.CustomData != null)
        {
            // In a real implementation, you'd merge the data properly
            // For now, we'll use custom data if provided, otherwise template data
            templateData = request.CustomData;
        }

        // Generate HTML from template
        var html = _generationService.GenerateHtmlFromTemplate(templateData!);

        // Generate document based on format
        byte[] documentBytes;
        string contentType;
        string fileExtension;

        switch (request.Format.ToLower())
        {
            case "pdf":
                documentBytes = await _generationService.GeneratePdfAsync(html, cancellationToken);
                contentType = "application/pdf";
                fileExtension = ".pdf";
                break;
            case "docx":
                documentBytes = await _generationService.GenerateDocxAsync(html, cancellationToken);
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                fileExtension = ".docx";
                break;
            case "html":
                documentBytes = System.Text.Encoding.UTF8.GetBytes(html);
                contentType = "text/html";
                fileExtension = ".html";
                break;
            default:
                return Result.Failure<ResumeDto>(new Error(
                    ErrorCodes.ValidationFailed,
                    "Invalid format specified"));
        }

        // Get version number
        var version = await _resumeRepository.GetNextVersionNumberAsync(request.UserId, request.Name, cancellationToken);

        // Save file
        var fileName = $"{request.Name}_v{version}{fileExtension}";
        using var stream = new MemoryStream(documentBytes);
        var uploadResult = await _fileStorageService.UploadAsync(
            stream,
            fileName,
            contentType,
            request.UserId,
            cancellationToken);

        // Create resume record
        var resume = new Resume
        {
            UserId = request.UserId,
            TemplateId = request.TemplateId,
            ApplicationId = request.ApplicationId,
            Name = request.Name,
            Description = request.Description,
            ResumeData = JsonSerializer.Serialize(templateData),
            FilePath = uploadResult.FilePath,
            FileFormat = request.Format.ToUpper(),
            GeneratedAt = DateTime.UtcNow,
            Version = version
        };

        await _resumeRepository.CreateAsync(resume, cancellationToken);

        _logger.LogInformation(
            "Resume generated successfully. Id: {ResumeId}, Name: {ResumeName}, Format: {Format}",
            resume.Id, resume.Name, resume.FileFormat);

        return Result.Success(new ResumeDto(
            resume.Id,
            resume.Name,
            resume.Description,
            resume.TemplateId,
            template.Name,
            resume.ApplicationId,
            applicationDetails,
            uploadResult.FileUrl,
            resume.FileFormat,
            resume.GeneratedAt,
            resume.Version,
            resume.CreatedAt
        ));
    }
}