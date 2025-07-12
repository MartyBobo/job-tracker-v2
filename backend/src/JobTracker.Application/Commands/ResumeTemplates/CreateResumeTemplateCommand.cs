using FluentValidation;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;
using System.Text.Json;

namespace JobTracker.Application.Commands.ResumeTemplates;

public record CreateResumeTemplateCommand : IRequest<Result<ResumeTemplateDto>>
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ResumeTemplateData TemplateData { get; init; } = null!;
}

public record ResumeTemplateDto(
    Guid Id,
    string Name,
    string? Description,
    ResumeTemplateData TemplateData,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ResumeTemplateData
{
    public ContactSection Contact { get; init; } = new();
    public string? Summary { get; init; }
    public List<ExperienceItem> Experience { get; init; } = new();
    public List<EducationItem> Education { get; init; } = new();
    public List<string> Skills { get; init; } = new();
    public List<ProjectItem> Projects { get; init; } = new();
    public List<CertificationItem> Certifications { get; init; } = new();
    public Dictionary<string, object>? CustomSections { get; init; }
}

public record ContactSection
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Location { get; init; }
    public string? LinkedIn { get; init; }
    public string? GitHub { get; init; }
    public string? Website { get; init; }
}

public record ExperienceItem
{
    public string JobTitle { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsCurrent { get; init; }
    public List<string> Responsibilities { get; init; } = new();
}

public record EducationItem
{
    public string Degree { get; init; } = string.Empty;
    public string School { get; init; } = string.Empty;
    public string? Location { get; init; }
    public DateTime? GraduationDate { get; init; }
    public string? GPA { get; init; }
}

public record ProjectItem
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<string> Technologies { get; init; } = new();
    public string? Link { get; init; }
}

public record CertificationItem
{
    public string Name { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public DateTime? IssueDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? CredentialId { get; init; }
}

public class CreateResumeTemplateCommandValidator : AbstractValidator<CreateResumeTemplateCommand>
{
    public CreateResumeTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
            
        RuleFor(x => x.Description)
            .MaximumLength(500);
            
        RuleFor(x => x.TemplateData)
            .NotNull()
            .Must(data => data.Contact != null)
            .WithMessage("Contact information is required");
            
        When(x => x.TemplateData != null, () =>
        {
            RuleFor(x => x.TemplateData.Contact.FullName)
                .NotEmpty()
                .WithMessage("Full name is required");
                
            RuleFor(x => x.TemplateData.Contact.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Valid email is required");
        });
    }
}

public class CreateResumeTemplateCommandHandler : IRequestHandler<CreateResumeTemplateCommand, Result<ResumeTemplateDto>>
{
    private readonly IResumeTemplateRepository _repository;
    private readonly ILogger<CreateResumeTemplateCommandHandler> _logger;

    public CreateResumeTemplateCommandHandler(
        IResumeTemplateRepository repository,
        ILogger<CreateResumeTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ResumeTemplateDto>> Handle(CreateResumeTemplateCommand request, CancellationToken cancellationToken)
    {
        // Check if template name already exists for user
        var nameExists = await _repository.NameExistsForUserAsync(request.UserId, request.Name, cancellationToken: cancellationToken);
        if (nameExists)
        {
            return Result.Failure<ResumeTemplateDto>(new Error(
                ErrorCodes.Conflict,
                $"A template with the name '{request.Name}' already exists"));
        }

        // Serialize template data to JSON
        var templateDataJson = JsonSerializer.Serialize(request.TemplateData);

        var template = new ResumeTemplate
        {
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            TemplateData = templateDataJson
        };

        await _repository.CreateAsync(template, cancellationToken);

        _logger.LogInformation(
            "Resume template created successfully. Id: {TemplateId}, Name: {TemplateName}, UserId: {UserId}",
            template.Id, template.Name, template.UserId);

        return Result.Success(new ResumeTemplateDto(
            template.Id,
            template.Name,
            template.Description,
            request.TemplateData,
            template.CreatedAt,
            template.UpdatedAt
        ));
    }
}