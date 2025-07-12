using FluentValidation;
using JobTracker.Application.DTOs.Auth;
using JobTracker.Application.Interfaces;
using JobTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared;

namespace JobTracker.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", Register)
            .WithName("Register")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPost("/login", Login)
            .WithName("Login")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
            
        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IValidator<RegisterRequest> validator,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<Program> logger)
    {
        logger.LogInformation("Registration attempt for email: {Email} at {Time}", 
            request.Email, DateTime.UtcNow);
        
        // Log request details for debugging
        logger.LogDebug("Registration request details: Email={Email}, FirstName={FirstName}, LastName={LastName}", 
            request.Email, request.FirstName, request.LastName);
        
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Registration validation failed for {Email}: {Errors}", 
                request.Email, 
                string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // Check if email already exists
        if (await userRepository.EmailExistsAsync(request.Email))
        {
            logger.LogWarning("Registration attempt for existing email: {Email}", request.Email);
            return Results.Problem(
                detail: "An account with this email already exists",
                statusCode: StatusCodes.Status409Conflict);
        }

        // Create user
        var user = new User
        {
            Email = request.Email.ToLower(),
            FullName = $"{request.FirstName} {request.LastName}".Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password)
        };

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays);

        // Save user
        await userRepository.CreateAsync(user);
        
        logger.LogInformation("New user registered successfully: {Email} with ID: {UserId} at {Time}", 
            user.Email, user.Id, DateTime.UtcNow);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.ExpirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName
            }
        };

        return Results.Created($"/api/users/{user.Id}", response);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<Program> logger)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // Find user
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for: {Email}", request.Email);
            return Results.Problem(
                detail: "Invalid email or password",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays);
        
        await userRepository.UpdateAsync(user);
        
        logger.LogInformation("User logged in: {Email}", user.Email);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.ExpirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName
            }
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenRequest request,
        IUserRepository userRepository,
        IJwtTokenService tokenService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<Program> logger)
    {
        if (!tokenService.ValidateRefreshToken(request.RefreshToken))
        {
            return Results.Problem(
                detail: "Invalid refresh token format",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Find user by refresh token
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (user == null)
        {
            logger.LogWarning("Invalid or expired refresh token used");
            return Results.Problem(
                detail: "Invalid or expired refresh token",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        // Generate new tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays);
        
        await userRepository.UpdateAsync(user);
        
        logger.LogInformation("Tokens refreshed for user: {Email}", user.Email);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.ExpirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName
            }
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        IUserRepository userRepository)
    {
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem(
                detail: "Invalid user token",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Results.Problem(
                detail: "User not found",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName
        };

        return Results.Ok(userDto);
    }
}