using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using JobTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Infrastructure.Repositories;

public class UploadRepository : IUploadRepository
{
    private readonly ApplicationDbContext _context;

    public UploadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Upload?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Uploads
            .Include(u => u.User)
            .Include(u => u.Application)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<List<Upload>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Uploads
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Upload>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        return await _context.Uploads
            .Where(u => u.ApplicationId == applicationId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Upload> CreateAsync(Upload upload, CancellationToken cancellationToken = default)
    {
        _context.Uploads.Add(upload);
        await _context.SaveChangesAsync(cancellationToken);
        return upload;
    }

    public async Task UpdateAsync(Upload upload, CancellationToken cancellationToken = default)
    {
        _context.Uploads.Update(upload);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Upload upload, CancellationToken cancellationToken = default)
    {
        // Soft delete
        upload.IsDeleted = true;
        upload.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Uploads.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<long> GetTotalStorageUsedByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Uploads
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .SumAsync(u => u.FileSize, cancellationToken);
    }
}