using System.Security.Claims;
using JobTracker.Application.Commands.Resumes;
using JobTracker.Application.Queries.Resumes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace JobTracker.API.Endpoints;

public static class ResumeEndpoints
{
    public static void MapResumeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/resumes")
            .WithTags("Resumes")
            .RequireAuthorization();

        group.MapPost("/generate", GenerateResume)
            .WithName("GenerateResume")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetUserResumes)
            .WithName("GetUserResumes")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetResumeById)
            .WithName("GetResumeById")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateResume)
            .WithName("UpdateResume")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteResume)
            .WithName("DeleteResume")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/preview", PreviewResume)
            .WithName("PreviewResume")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/export", ExportResume)
            .WithName("ExportResume")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GenerateResume(
        GenerateResumeCommand command,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var commandWithUser = command with { UserId = Guid.Parse(userId) };
        var result = await mediator.Send(commandWithUser);

        return result.IsSuccess
            ? Results.Created($"/api/resumes/{result.Value.Id}", result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetUserResumes(
        ISender mediator,
        HttpContext httpContext,
        [FromQuery] Guid? templateId = null,
        [FromQuery] Guid? applicationId = null)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetUserResumesQuery
        {
            UserId = Guid.Parse(userId),
            TemplateId = templateId,
            ApplicationId = applicationId
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetResumeById(
        Guid id,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetResumeByIdQuery
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> UpdateResume(
        Guid id,
        UpdateResumeCommand command,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var commandWithIds = command with 
        { 
            Id = id,
            UserId = Guid.Parse(userId) 
        };

        var result = await mediator.Send(commandWithIds);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> DeleteResume(
        Guid id,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var command = new DeleteResumeCommand
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.NoContent()
            : HandleError(result.Error!);
    }

    private static async Task<IResult> PreviewResume(
        PreviewResumeQuery query,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var queryWithUser = query with { UserId = Guid.Parse(userId) };
        var result = await mediator.Send(queryWithUser);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> ExportResume(
        Guid id,
        ISender mediator,
        HttpContext httpContext,
        [FromQuery] string? format = null)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        // Get resume to verify ownership
        var query = new GetResumeByIdQuery
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
            return HandleError(result.Error!);

        var resume = result.Value;

        // If no format specified or format matches the stored format, redirect to the file URL
        if (string.IsNullOrEmpty(format) || 
            format.ToUpper() == resume.FileFormat ||
            !string.IsNullOrEmpty(resume.FilePath))
        {
            if (!string.IsNullOrEmpty(resume.FilePath))
                return Results.Redirect(resume.FilePath);
        }

        // Otherwise, we'd need to regenerate in the requested format
        // For now, just return the existing file
        return Results.BadRequest(new ProblemDetails
        {
            Title = "Format Conversion Not Implemented",
            Detail = "Converting between formats is not yet implemented. Please download the original format.",
            Status = StatusCodes.Status400BadRequest
        });
    }

    private static IResult HandleError(Error error)
    {
        return error.Code switch
        {
            Shared.Errors.ErrorCodes.NotFound => Results.NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            }),
            Shared.Errors.ErrorCodes.Forbidden => Results.Forbid(),
            Shared.Errors.ErrorCodes.JobApplicationNotFound => Results.NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            }),
            Shared.Errors.ErrorCodes.ValidationFailed => Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = error.Message,
                Status = StatusCodes.Status400BadRequest
            }),
            _ => Results.Problem(new ProblemDetails
            {
                Title = "An error occurred",
                Detail = error.Message,
                Status = StatusCodes.Status500InternalServerError
            })
        };
    }
}