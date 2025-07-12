using JobTracker.Domain.Entities;

namespace JobTracker.Application.Interfaces;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobApplication?> GetByIdWithInterviewsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobApplication?> GetByIdWithAllRelatedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<JobApplication>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<JobApplication> CreateAsync(JobApplication application, CancellationToken cancellationToken = default);
    Task UpdateAsync(JobApplication application, CancellationToken cancellationToken = default);
    Task DeleteAsync(JobApplication application, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}