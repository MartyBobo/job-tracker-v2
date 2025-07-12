using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

public class InterviewRepository : IInterviewRepository
{
    private readonly ApplicationDbContext _context;

    public InterviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Interview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Interviews
            .Include(i => i.Application)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<List<Interview>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _context.Interviews
            .Where(i => i.ApplicationId == applicationId)
            .OrderBy(i => i.InterviewDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Interview> CreateAsync(Interview interview, CancellationToken cancellationToken = default)
    {
        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync(cancellationToken);
        return interview;
    }

    public async Task UpdateAsync(Interview interview, CancellationToken cancellationToken = default)
    {
        _context.Interviews.Update(interview);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Interview interview, CancellationToken cancellationToken = default)
    {
        // Soft delete
        interview.IsDeleted = true;
        interview.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasScheduleConflictAsync(
        Guid userId, 
        DateTime interviewDate, 
        TimeSpan duration, 
        Guid? excludeInterviewId = null, 
        CancellationToken cancellationToken = default)
    {
        var endTime = interviewDate.Add(duration);
        
        var query = _context.Interviews
            .Include(i => i.Application)
            .Where(i => i.Application.UserId == userId && 
                       i.InterviewDate < endTime &&
                       i.InterviewDate.AddHours(1) > interviewDate); // Assume 1 hour default duration
        
        if (excludeInterviewId.HasValue)
        {
            query = query.Where(i => i.Id != excludeInterviewId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}