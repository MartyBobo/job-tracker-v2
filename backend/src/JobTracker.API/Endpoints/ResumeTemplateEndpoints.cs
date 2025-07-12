using System.Security.Claims;
using JobTracker.Application.Commands.ResumeTemplates;
using JobTracker.Application.Queries.ResumeTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace JobTracker.API.Endpoints;

public static class ResumeTemplateEndpoints
{
    public static void MapResumeTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/resume-templates")
            .WithTags("Resume Templates")
            .RequireAuthorization();

        group.MapPost("/", CreateResumeTemplate)
            .WithName("CreateResumeTemplate")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetUserResumeTemplates)
            .WithName("GetUserResumeTemplates")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetResumeTemplateById)
            .WithName("GetResumeTemplateById")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateResumeTemplate)
            .WithName("UpdateResumeTemplate")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteResumeTemplate)
            .WithName("DeleteResumeTemplate")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/clone", CloneResumeTemplate)
            .WithName("CloneResumeTemplate")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateResumeTemplate(
        CreateResumeTemplateCommand command,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var commandWithUser = command with { UserId = Guid.Parse(userId) };
        var result = await mediator.Send(commandWithUser);

        return result.IsSuccess
            ? Results.Created($"/api/resume-templates/{result.Value.Id}", result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetUserResumeTemplates(
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetUserResumeTemplatesQuery
        {
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetResumeTemplateById(
        Guid id,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetResumeTemplateByIdQuery
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> UpdateResumeTemplate(
        Guid id,
        UpdateResumeTemplateCommand command,
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

    private static async Task<IResult> DeleteResumeTemplate(
        Guid id,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var command = new DeleteResumeTemplateCommand
        {
            Id = id,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.NoContent()
            : HandleError(result.Error!);
    }

    private static async Task<IResult> CloneResumeTemplate(
        Guid id,
        [FromBody] CloneResumeTemplateRequest request,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var command = new CloneResumeTemplateCommand
        {
            Id = id,
            UserId = Guid.Parse(userId),
            NewName = request.NewName
        };

        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.Created($"/api/resume-templates/{result.Value.Id}", result.Value)
            : HandleError(result.Error!);
    }

    private static IResult HandleError(Error error)
    {
        return error.Code switch
        {
            Shared.Errors.ErrorCodes.NotFound => Results.NotFound(new ProblemDetails
            {
                Title = "Template Not Found",
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            }),
            Shared.Errors.ErrorCodes.Forbidden => Results.Forbid(),
            Shared.Errors.ErrorCodes.Conflict => Results.Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = error.Message,
                Status = StatusCodes.Status409Conflict
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

public record CloneResumeTemplateRequest(string NewName);