using JobTracker.Application.Commands.ResumeTemplates;
using JobTracker.Application.Interfaces;
using MediatR;
using Shared.Results;
using System.Text.Json;

namespace JobTracker.Application.Queries.ResumeTemplates;

public record GetUserResumeTemplatesQuery : IRequest<Result<List<ResumeTemplateDto>>>
{
    public Guid UserId { get; init; }
}

public class GetUserResumeTemplatesQueryHandler : IRequestHandler<GetUserResumeTemplatesQuery, Result<List<ResumeTemplateDto>>>
{
    private readonly IResumeTemplateRepository _repository;

    public GetUserResumeTemplatesQueryHandler(IResumeTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ResumeTemplateDto>>> Handle(GetUserResumeTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        var templateDtos = templates.Select(template =>
        {
            var templateData = JsonSerializer.Deserialize<ResumeTemplateData>(template.TemplateData);
            return new ResumeTemplateDto(
                template.Id,
                template.Name,
                template.Description,
                templateData!,
                template.CreatedAt,
                template.UpdatedAt
            );
        }).ToList();

        return Result.Success(templateDtos);
    }
}