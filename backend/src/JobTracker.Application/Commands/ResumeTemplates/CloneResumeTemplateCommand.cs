using FluentValidation;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;
using System.Text.Json;

namespace JobTracker.Application.Commands.ResumeTemplates;

public record CloneResumeTemplateCommand : IRequest<Result<ResumeTemplateDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string NewName { get; init; } = string.Empty;
}

public class CloneResumeTemplateCommandValidator : AbstractValidator<CloneResumeTemplateCommand>
{
    public CloneResumeTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewName)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public class CloneResumeTemplateCommandHandler : IRequestHandler<CloneResumeTemplateCommand, Result<ResumeTemplateDto>>
{
    private readonly IResumeTemplateRepository _repository;
    private readonly ILogger<CloneResumeTemplateCommandHandler> _logger;

    public CloneResumeTemplateCommandHandler(
        IResumeTemplateRepository repository,
        ILogger<CloneResumeTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<ResumeTemplateDto>> Handle(CloneResumeTemplateCommand request, CancellationToken cancellationToken)
    {
        var sourceTemplate = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (sourceTemplate == null)
        {
            return Result.Failure<ResumeTemplateDto>(new Error(
                ErrorCodes.NotFound,
                "Resume template not found"));
        }

        // Verify ownership
        if (sourceTemplate.UserId != request.UserId)
        {
            return Result.Failure<ResumeTemplateDto>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to clone this template"));
        }

        // Check if new name already exists
        var nameExists = await _repository.NameExistsForUserAsync(
            request.UserId, 
            request.NewName, 
            cancellationToken: cancellationToken);
            
        if (nameExists)
        {
            return Result.Failure<ResumeTemplateDto>(new Error(
                ErrorCodes.Conflict,
                $"A template with the name '{request.NewName}' already exists"));
        }

        // Create clone
        var clonedTemplate = new ResumeTemplate
        {
            UserId = request.UserId,
            Name = request.NewName,
            Description = sourceTemplate.Description != null 
                ? $"Cloned from: {sourceTemplate.Name}" 
                : null,
            TemplateData = sourceTemplate.TemplateData
        };

        await _repository.CreateAsync(clonedTemplate, cancellationToken);

        // Deserialize template data for response
        var templateData = JsonSerializer.Deserialize<ResumeTemplateData>(clonedTemplate.TemplateData);

        _logger.LogInformation(
            "Resume template cloned successfully. SourceId: {SourceId}, NewId: {NewId}, Name: {NewName}",
            sourceTemplate.Id, clonedTemplate.Id, clonedTemplate.Name);

        return Result.Success(new ResumeTemplateDto(
            clonedTemplate.Id,
            clonedTemplate.Name,
            clonedTemplate.Description,
            templateData!,
            clonedTemplate.CreatedAt,
            clonedTemplate.UpdatedAt
        ));
    }
}