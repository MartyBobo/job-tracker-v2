using JobTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.API.Endpoints;

public static class DebugEndpoints
{
    public static void MapDebugEndpoints(this IEndpointRouteBuilder app)
    {
        // Only enable debug endpoints in development
        if (!app.ServiceProvider.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            return;

        var group = app.MapGroup("/api/debug")
            .WithTags("Debug")
            .WithDescription("Debug endpoints - only available in development");

        group.MapGet("/users", GetAllUsers)
            .WithName("GetAllUsersDebug")
            .Produces<DebugUsersResponse>(StatusCodes.Status200OK)
            .WithDescription("Lists all users in the database (Development Only)");
    }

    private static async Task<IResult> GetAllUsers(
        IUserRepository userRepository,
        ILogger<Program> logger)
    {
        logger.LogInformation("Debug: Fetching all users");
        
        var users = await userRepository.GetAllAsync();
        
        var response = new DebugUsersResponse
        {
            TotalUsers = users.Count(),
            Users = users.Select(u => new DebugUserInfo
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt,
                HasRefreshToken = !string.IsNullOrEmpty(u.RefreshToken),
                RefreshTokenExpiry = u.RefreshTokenExpiryTime
            }).ToList(),
            GeneratedAt = DateTime.UtcNow
        };

        logger.LogInformation("Debug: Found {Count} users", response.TotalUsers);
        
        return Results.Ok(response);
    }
}

public class DebugUsersResponse
{
    public int TotalUsers { get; set; }
    public List<DebugUserInfo> Users { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class DebugUserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool HasRefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}