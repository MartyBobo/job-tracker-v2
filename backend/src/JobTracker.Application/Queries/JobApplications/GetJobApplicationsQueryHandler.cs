using JobTracker.Application.DTOs.JobApplications;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using MediatR;
using Shared.Results;

namespace JobTracker.Application.Queries.JobApplications;

public class GetJobApplicationsQueryHandler : IRequestHandler<GetJobApplicationsQuery, Result<List<JobApplicationDto>>>
{
    private readonly IJobApplicationRepository _repository;

    public GetJobApplicationsQueryHandler(IJobApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<JobApplicationDto>>> Handle(
        GetJobApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        var applications = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        // Apply filters
        var query = applications.AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(a => 
                a.JobTitle.ToLower().Contains(searchTerm) ||
                a.CompanyName.ToLower().Contains(searchTerm) ||
                (a.Notes != null && a.Notes.ToLower().Contains(searchTerm)));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.AppliedDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(a => a.AppliedDate <= request.EndDate.Value);
        }

        if (request.IsRemote.HasValue)
        {
            query = query.Where(a => a.IsRemote == request.IsRemote.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "jobtitle" => request.SortDescending 
                ? query.OrderByDescending(a => a.JobTitle)
                : query.OrderBy(a => a.JobTitle),
            "companyname" => request.SortDescending
                ? query.OrderByDescending(a => a.CompanyName)
                : query.OrderBy(a => a.CompanyName),
            "status" => request.SortDescending
                ? query.OrderByDescending(a => a.Status)
                : query.OrderBy(a => a.Status),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt),
            _ => request.SortDescending
                ? query.OrderByDescending(a => a.AppliedDate)
                : query.OrderBy(a => a.AppliedDate)
        };

        var results = query.Select(a => new JobApplicationDto
        {
            Id = a.Id,
            JobTitle = a.JobTitle,
            CompanyName = a.CompanyName,
            ContactEmail = a.ContactEmail,
            ContactPhone = a.ContactPhone,
            IsRemote = a.IsRemote,
            SelfSourced = a.SelfSourced,
            AppliedDate = a.AppliedDate,
            Status = a.Status,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            InterviewCount = a.Interviews.Count,
            UploadCount = a.Uploads.Count
        }).ToList();

        return Result.Success(results);
    }
}