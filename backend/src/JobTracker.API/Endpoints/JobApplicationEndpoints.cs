using System.Security.Claims;
using JobTracker.Application.Commands.JobApplications;
using JobTracker.Application.Queries.JobApplications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace JobTracker.API.Endpoints;

public static class JobApplicationEndpoints
{
    public static void MapJobApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/applications")
            .WithTags("Job Applications")
            .RequireAuthorization();

        group.MapPost("/", CreateJobApplication)
            .WithName("CreateJobApplication")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetJobApplications)
            .WithName("GetJobApplications")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetJobApplicationById)
            .WithName("GetJobApplicationById")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateJobApplication)
            .WithName("UpdateJobApplication")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteJobApplication)
            .WithName("DeleteJobApplication")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateJobApplication(
        CreateJobApplicationCommand command,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var commandWithUser = command with { UserId = Guid.Parse(userId) };
        var result = await mediator.Send(commandWithUser);

        return result.IsSuccess
            ? Results.Created($"/api/applications/{result.Value.Id}", result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetJobApplications(
        ISender mediator,
        HttpContext httpContext,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isRemote = null)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetJobApplicationsQuery
        {
            UserId = Guid.Parse(userId),
            Status = status != null ? Enum.Parse<JobTracker.Domain.Enums.ApplicationStatus>(status) : null,
            SearchTerm = search,
            IsRemote = isRemote
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetJobApplicationById(
        Guid id,
        ISender mediator,
        HttpContext httpContext,
        [FromQuery] bool includeInterviews = false)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetJobApplicationByIdQuery
        {
            Id = id,
            UserId = Guid.Parse(userId),
            IncludeInterviews = includeInterviews
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> UpdateJobApplication(
        Guid id,
        UpdateJobApplicationCommand command,
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
            ? Results.NoContent()
            : HandleError(result.Error!);
    }

    private static async Task<IResult> DeleteJobApplication(
        Guid id,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var command = new DeleteJobApplicationCommand
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.NoContent()
            : HandleError(result.Error!);
    }

    private static IResult HandleError(Error error)
    {
        return error.Code switch
        {
            Shared.Errors.ErrorCodes.NotFound or 
            Shared.Errors.ErrorCodes.JobApplicationNotFound => Results.NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            }),
            Shared.Errors.ErrorCodes.Forbidden => Results.Forbid(),
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