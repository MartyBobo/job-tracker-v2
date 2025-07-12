using FluentValidation;
using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;
using System.Text.Json;

namespace JobTracker.Application.Commands.ResumeTemplates;

public record UpdateResumeTemplateCommand : IRequest<Result<ResumeTemplateDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ResumeTemplateData TemplateData { get; init; } = null!;
}

public class UpdateResumeTemplateCommandValidator : AbstractValidator<UpdateResumeTemplateCommand>
{
    public UpdateResumeTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
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

public class UpdateResumeTemplateCommandHandler : IRequestHandler<UpdateResumeTemplateCommand, Result<ResumeTemplateDto>>
{
    private readonly IResumeTemplateRepository _repository;
    private readonly ILogger<UpdateResumeTemplateCommandHandler> _logger;

    public UpdateResumeTemplateCommandHandler(
        IResumeTemplateRepository repository,
        ILogger<UpdateResumeTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ResumeTemplateDto>> Handle(UpdateResumeTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (template == null)
        {
            return Result.Failure<ResumeTemplateDto>(new Error(
                ErrorCodes.NotFound,
                "Resume template not found"));
        }

        // Verify ownership
        if (template.UserId != request.UserId)
        {
            return Result.Failure<ResumeTemplateDto>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to update this template"));
        }

        // Check if new name conflicts with existing template
        if (template.Name != request.Name)
        {
            var nameExists = await _repository.NameExistsForUserAsync(
                request.UserId, 
                request.Name, 
                excludeId: request.Id, 
                cancellationToken: cancellationToken);
                
            if (nameExists)
            {
                return Result.Failure<ResumeTemplateDto>(new Error(
                    ErrorCodes.Conflict,
                    $"A template with the name '{request.Name}' already exists"));
            }
        }

        // Update template
        template.Name = request.Name;
        template.Description = request.Description;
        template.TemplateData = JsonSerializer.Serialize(request.TemplateData);

        await _repository.UpdateAsync(template, cancellationToken);

        _logger.LogInformation(
            "Resume template updated successfully. Id: {TemplateId}, Name: {TemplateName}",
            template.Id, template.Name);

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