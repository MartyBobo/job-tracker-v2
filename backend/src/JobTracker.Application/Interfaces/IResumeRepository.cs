using JobTracker.Domain.Entities;

namespace JobTracker.Application.Interfaces;

public interface IResumeRepository
{
    Task<Resume?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Resume>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Resume>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<List<Resume>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<Resume> CreateAsync(Resume resume, CancellationToken cancellationToken = default);
    Task UpdateAsync(Resume resume, CancellationToken cancellationToken = default);
    Task DeleteAsync(Resume resume, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionNumberAsync(Guid userId, string name, CancellationToken cancellationToken = default);
}