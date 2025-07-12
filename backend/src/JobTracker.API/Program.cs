using System.Text;
using FluentValidation;
using JobTracker.API.Endpoints;
using JobTracker.Application.Interfaces;
using JobTracker.Infrastructure.Persistence;
using JobTracker.Infrastructure.Repositories;
using JobTracker.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/jobtracker-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting JobTracker API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "JobTracker API", 
            Version = "v1",
            Description = "API for tracking job applications and managing resumes"
        });
        
        // Add JWT authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Configure JSON serialization to use camelCase
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

    // Add DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configure JWT settings
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    builder.Services.Configure<JwtSettings>(jwtSettings);

    // Add authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"))),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // Register MediatR
    builder.Services.AddMediatR(cfg => {
        cfg.RegisterServicesFromAssembly(typeof(JobTracker.Application.Commands.JobApplications.CreateJobApplicationCommandHandler).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(JobTracker.Application.Behaviors.LoggingBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(JobTracker.Application.Behaviors.ValidationBehavior<,>));
    });

    // Register services
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
    builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
    builder.Services.AddScoped<IUploadRepository, UploadRepository>();
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
    builder.Services.AddScoped<IFileValidationService, FileValidationService>();
    builder.Services.AddScoped<IResumeTemplateRepository, ResumeTemplateRepository>();
    builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
    builder.Services.AddScoped<IResumeGenerationService, ResumeGenerationService>();

    // Configure file validation options
    builder.Services.Configure<FileValidationOptions>(
        builder.Configuration.GetSection("FileValidation"));

    // Register validators
    builder.Services.AddValidatorsFromAssemblyContaining<JobTracker.Application.Validators.RegisterRequestValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<JobTracker.Application.Commands.JobApplications.CreateJobApplicationCommandValidator>();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3100")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .WithExposedHeaders("Content-Disposition");
        });
        
        options.AddPolicy("ProductionCors", policy =>
        {
            var frontendUrl = builder.Configuration["Frontend:Url"] ?? 
                             Environment.GetEnvironmentVariable("FRONTEND_URL") ?? 
                             "https://jobtracker.app";
            
            policy.WithOrigins(frontendUrl)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .WithExposedHeaders("Content-Disposition");
        });
    });

    var app = builder.Build();

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    
    // Use appropriate CORS policy based on environment
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("DevelopmentCors");
    }
    else
    {
        app.UseCors("ProductionCors");
    }
    
    app.UseSerilogRequestLogging();

    // Configure static file serving for uploads
    var uploadsPath = builder.Configuration["FileStorage:BasePath"] ?? "uploads";
    if (!Path.IsPathRooted(uploadsPath))
    {
        uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
    }
    
    // Ensure the uploads directory exists
    Directory.CreateDirectory(uploadsPath);
    
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
        RequestPath = "/files",
        OnPrepareResponse = ctx =>
        {
            // Add security headers
            ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            
            // Cache for 1 hour
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
        }
    });
    
    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
        .WithName("HealthCheck")
        .WithOpenApi()
        .AllowAnonymous();

    // Map endpoints
    app.MapAuthEndpoints();
    app.MapJobApplicationEndpoints();
    app.MapDebugEndpoints();
    app.MapFileUploadEndpoints();
    app.MapInterviewEndpoints();
    app.MapResumeTemplateEndpoints();
    app.MapResumeEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}