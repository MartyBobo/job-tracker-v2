using JobTracker.Domain.Entities;

namespace JobTracker.Application.Interfaces;

public interface IResumeTemplateRepository
{
    Task<ResumeTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ResumeTemplate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ResumeTemplate> CreateAsync(ResumeTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(ResumeTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(ResumeTemplate template, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsForUserAsync(Guid userId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}