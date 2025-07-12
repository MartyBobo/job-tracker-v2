using JobTracker.Application.Commands.ResumeTemplates;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Errors;
using Shared.Results;
using System.Text.Json;

namespace JobTracker.Application.Queries.ResumeTemplates;

public record GetResumeTemplateByIdQuery : IRequest<Result<ResumeTemplateDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}

public class GetResumeTemplateByIdQueryHandler : IRequestHandler<GetResumeTemplateByIdQuery, Result<ResumeTemplateDto>>
{
    private readonly IResumeTemplateRepository _repository;

    public GetResumeTemplateByIdQueryHandler(IResumeTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ResumeTemplateDto>> Handle(GetResumeTemplateByIdQuery request, CancellationToken cancellationToken)
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
                "You don't have permission to view this template"));
        }

        // Deserialize template data
        var templateData = JsonSerializer.Deserialize<ResumeTemplateData>(template.TemplateData);

        return Result.Success(new ResumeTemplateDto(
            template.Id,
            template.Name,
            template.Description,
            templateData!,
            template.CreatedAt,
            template.UpdatedAt
        ));
    }
}