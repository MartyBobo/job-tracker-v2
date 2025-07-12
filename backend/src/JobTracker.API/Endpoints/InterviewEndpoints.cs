using System.Security.Claims;
using JobTracker.Application.Commands.Interviews;
using JobTracker.Application.Queries.Interviews;
using JobTracker.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace JobTracker.API.Endpoints;

public static class InterviewEndpoints
{
    public static void MapInterviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/interviews")
            .WithTags("Interviews")
            .RequireAuthorization();

        group.MapPost("/", CreateInterview)
            .WithName("CreateInterview")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetInterviewById)
            .WithName("GetInterviewById")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateInterview)
            .WithName("UpdateInterview")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteInterview)
            .WithName("DeleteInterview")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/upcoming", GetUpcomingInterviews)
            .WithName("GetUpcomingInterviews")
            .Produces(StatusCodes.Status200OK);

        // Application-specific interview endpoints
        app.MapGet("/api/applications/{applicationId:guid}/interviews", GetInterviewsByApplication)
            .WithName("GetInterviewsByApplication")
            .RequireAuthorization()
            .WithTags("Interviews")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateInterview(
        CreateInterviewCommand command,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var commandWithUser = command with { UserId = Guid.Parse(userId) };
        var result = await mediator.Send(commandWithUser);

        return result.IsSuccess
            ? Results.Created($"/api/interviews/{result.Value.Id}", result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetInterviewById(
        Guid id,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetInterviewByIdQuery
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> UpdateInterview(
        Guid id,
        UpdateInterviewCommand command,
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

    private static async Task<IResult> DeleteInterview(
        Guid id,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var command = new DeleteInterviewCommand
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.NoContent()
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetUpcomingInterviews(
        ISender mediator,
        HttpContext httpContext,
        [FromQuery] int daysAhead = 30)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetUpcomingInterviewsQuery
        {
            UserId = Guid.Parse(userId),
            DaysAhead = daysAhead
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetInterviewsByApplication(
        Guid applicationId,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetInterviewsByApplicationQuery
        {
            ApplicationId = applicationId,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static IResult HandleError(Error error)
    {
        return error.Code switch
        {
            Shared.Errors.ErrorCodes.InterviewNotFound => Results.NotFound(new ProblemDetails
            {
                Title = "Interview Not Found",
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            }),
            Shared.Errors.ErrorCodes.JobApplicationNotFound => Results.NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
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