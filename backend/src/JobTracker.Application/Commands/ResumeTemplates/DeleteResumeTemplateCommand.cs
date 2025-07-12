using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.ResumeTemplates;

public record DeleteResumeTemplateCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}

public class DeleteResumeTemplateCommandHandler : IRequestHandler<DeleteResumeTemplateCommand, Result<Unit>>
{
    private readonly IResumeTemplateRepository _repository;
    private readonly ILogger<DeleteResumeTemplateCommandHandler> _logger;

    public DeleteResumeTemplateCommandHandler(
        IResumeTemplateRepository repository,
        ILogger<DeleteResumeTemplateCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteResumeTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (template == null)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.NotFound,
                "Resume template not found"));
        }

        // Verify ownership
        if (template.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to delete this template"));
        }

        await _repository.DeleteAsync(template, cancellationToken);

        _logger.LogInformation(
            "Resume template deleted successfully. Id: {TemplateId}, Name: {TemplateName}",
            template.Id, template.Name);

        return Result.Success(Unit.Value);
    }
}