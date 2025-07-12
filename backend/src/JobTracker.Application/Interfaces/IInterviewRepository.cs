using JobTracker.Domain.Entities;

namespace JobTracker.Application.Interfaces;

public interface IInterviewRepository
{
    Task<Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Interview>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<Interview> CreateAsync(Interview interview, CancellationToken cancellationToken = default);
    Task UpdateAsync(Interview interview, CancellationToken cancellationToken = default);
    Task DeleteAsync(Interview interview, CancellationToken cancellationToken = default);
    Task<bool> HasScheduleConflictAsync(Guid userId, DateTime interviewDate, TimeSpan duration, Guid? excludeInterviewId = null, CancellationToken cancellationToken = default);
}