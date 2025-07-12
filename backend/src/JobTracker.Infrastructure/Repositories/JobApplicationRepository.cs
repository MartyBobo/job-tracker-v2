using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public JobApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<JobApplication?> GetByIdWithInterviewsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobApplications
            .Include(j => j.Interviews.OrderBy(i => i.InterviewDate))
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<JobApplication?> GetByIdWithAllRelatedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobApplications
            .Include(j => j.Interviews.OrderBy(i => i.InterviewDate))
            .Include(j => j.Uploads)
            .Include(j => j.User)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<List<JobApplication>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.JobApplications
            .Include(j => j.Interviews)
            .Include(j => j.Uploads)
            .Where(j => j.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobApplication> CreateAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);
        return application;
    }

    public async Task UpdateAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        _context.JobApplications.Update(application);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        // Soft delete
        application.IsDeleted = true;
        application.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobApplications
            .AnyAsync(j => j.Id == id, cancellationToken);
    }
}