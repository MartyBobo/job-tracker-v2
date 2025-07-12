using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly ApplicationDbContext _context;

    public ResumeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Resume?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .Include(r => r.User)
            .Include(r => r.Template)
            .Include(r => r.Application)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Resume>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .Include(r => r.Template)
            .Include(r => r.Application)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Resume>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .Include(r => r.Template)
            .Where(r => r.ApplicationId == applicationId)
            .OrderByDescending(r => r.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Resume>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .Include(r => r.Application)
            .Where(r => r.TemplateId == templateId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Resume> CreateAsync(Resume resume, CancellationToken cancellationToken = default)
    {
        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync(cancellationToken);
        return resume;
    }

    public async Task UpdateAsync(Resume resume, CancellationToken cancellationToken = default)
    {
        _context.Resumes.Update(resume);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Resume resume, CancellationToken cancellationToken = default)
    {
        // Soft delete
        resume.IsDeleted = true;
        resume.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetNextVersionNumberAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        var maxVersion = await _context.Resumes
            .Where(r => r.UserId == userId && r.Name == name)
            .MaxAsync(r => (int?)r.Version, cancellationToken) ?? 0;
        
        return maxVersion + 1;
    }
}