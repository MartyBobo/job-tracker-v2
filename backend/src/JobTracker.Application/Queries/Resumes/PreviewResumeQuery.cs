using JobTracker.Application.Commands.ResumeTemplates;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Errors;
using Shared.Results;
using System.Text.Json;

namespace JobTracker.Application.Queries.Resumes;

public record PreviewResumeQuery : IRequest<Result<PreviewResumeResult>>
{
    public Guid UserId { get; init; }
    public Guid TemplateId { get; init; }
    public ResumeTemplateData? CustomData { get; init; }
}

public record PreviewResumeResult(
    string Html,
    string TemplateName
);

public class PreviewResumeQueryHandler : IRequestHandler<PreviewResumeQuery, Result<PreviewResumeResult>>
{
    private readonly IResumeTemplateRepository _templateRepository;
    private readonly IResumeGenerationService _generationService;

    public PreviewResumeQueryHandler(
        IResumeTemplateRepository templateRepository,
        IResumeGenerationService generationService)
    {
        _templateRepository = templateRepository;
        _generationService = generationService;
    }

    public async Task<Result<PreviewResumeResult>> Handle(PreviewResumeQuery request, CancellationToken cancellationToken)
    {
        // Get template
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template == null || template.UserId != request.UserId)
        {
            return Result.Failure<PreviewResumeResult>(new Error(
                ErrorCodes.NotFound,
                "Resume template not found"));
        }

        // Use custom data if provided, otherwise use template data
        var templateData = request.CustomData ?? 
            JsonSerializer.Deserialize<ResumeTemplateData>(template.TemplateData);

        // Generate HTML preview
        var html = _generationService.GenerateHtmlFromTemplate(templateData!);

        return Result.Success(new PreviewResumeResult(html, template.Name));
    }
}