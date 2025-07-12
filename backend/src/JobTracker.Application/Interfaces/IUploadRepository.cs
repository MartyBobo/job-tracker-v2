using JobTracker.Domain.Entities;

namespace JobTracker.Application.Interfaces;

public interface IUploadRepository
{
    Task<Upload?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Upload>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Upload>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<Upload> CreateAsync(Upload upload, CancellationToken cancellationToken = default);
    Task UpdateAsync(Upload upload, CancellationToken cancellationToken = default);
    Task DeleteAsync(Upload upload, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<long> GetTotalStorageUsedByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}