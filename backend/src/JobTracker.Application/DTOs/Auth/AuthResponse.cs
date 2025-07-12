using System.Linq;

namespace JobTracker.Application.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    public string FirstName => FullName.Split(' ', 2).FirstOrDefault() ?? string.Empty;
    public string LastName => FullName.Split(' ', 2).Skip(1).FirstOrDefault() ?? string.Empty;
}