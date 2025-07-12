using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

public class ResumeTemplateRepository : IResumeTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public ResumeTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResumeTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ResumeTemplates
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<List<ResumeTemplate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ResumeTemplates
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResumeTemplate> CreateAsync(ResumeTemplate template, CancellationToken cancellationToken = default)
    {
        _context.ResumeTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task UpdateAsync(ResumeTemplate template, CancellationToken cancellationToken = default)
    {
        _context.ResumeTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ResumeTemplate template, CancellationToken cancellationToken = default)
    {
        // Soft delete
        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ResumeTemplates.AnyAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsForUserAsync(Guid userId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ResumeTemplates
            .Where(rt => rt.UserId == userId && rt.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(rt => rt.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}