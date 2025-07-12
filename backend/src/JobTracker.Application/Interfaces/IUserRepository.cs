using JobTracker.Domain.Entities;

namespace JobTracker.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
}