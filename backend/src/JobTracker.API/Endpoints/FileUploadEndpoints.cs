using System.Security.Claims;
using JobTracker.Application.Commands.Uploads;
using JobTracker.Application.Queries.Uploads;
using JobTracker.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace JobTracker.API.Endpoints;

public static class FileUploadEndpoints
{
    public static void MapFileUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files")
            .WithTags("File Uploads")
            .RequireAuthorization()
            .DisableAntiforgery(); // Required for file uploads

        group.MapPost("/upload", UploadFile)
            .WithName("UploadFile")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Accepts<IFormFile>("multipart/form-data");

        group.MapGet("/", GetUserFiles)
            .WithName("GetUserFiles")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{fileId:guid}/download", DownloadFile)
            .WithName("DownloadFile")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{fileId:guid}", DeleteFile)
            .WithName("DeleteFile")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> UploadFile(
        IFormFile file,
        ISender mediator,
        HttpContext httpContext,
        [FromForm] Guid? applicationId = null,
        [FromForm] DocumentType documentType = DocumentType.Other,
        [FromForm] string? description = null)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid File",
                Detail = "No file was uploaded",
                Status = StatusCodes.Status400BadRequest
            });
        }

        using var fileStream = file.OpenReadStream();
        
        var command = new UploadFileCommand
        {
            FileStream = fileStream,
            FileName = file.FileName,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileSize = file.Length,
            UserId = Guid.Parse(userId),
            ApplicationId = applicationId,
            DocumentType = documentType,
            Description = description
        };

        var result = await mediator.Send(command);

        return result.IsSuccess
            ? Results.Created($"/api/files/{result.Value.Id}/download", result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> GetUserFiles(
        ISender mediator,
        HttpContext httpContext,
        [FromQuery] Guid? applicationId = null,
        [FromQuery] DocumentType? documentType = null)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetUserUploadsQuery
        {
            UserId = Guid.Parse(userId),
            ApplicationId = applicationId,
            DocumentType = documentType
        };

        var result = await mediator.Send(query);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : HandleError(result.Error!);
    }

    private static async Task<IResult> DownloadFile(
        Guid fileId,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new DownloadFileQuery
        {
            FileId = fileId,
            UserId = Guid.Parse(userId)
        };

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
            return HandleError(result.Error!);

        var download = result.Value;
        
        return Results.File(
            download.FileStream,
            download.ContentType,
            download.FileName,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> DeleteFile(
        Guid fileId,
        ISender mediator,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var command = new DeleteFileCommand
        {
            FileId = fileId,
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
            Shared.Errors.ErrorCodes.FileNotFound => Results.NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = error.Message,
                Status = StatusCodes.Status404NotFound
            }),
            Shared.Errors.ErrorCodes.Forbidden => Results.Forbid(),
            Shared.Errors.ErrorCodes.ValidationFailed or
            Shared.Errors.ErrorCodes.FileTooLarge or
            Shared.Errors.ErrorCodes.FileTypeNotAllowed or
            Shared.Errors.ErrorCodes.InvalidFileType => Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Failed",
                Detail = error.Message,
                Status = StatusCodes.Status400BadRequest
            }),
            Shared.Errors.ErrorCodes.StorageQuotaExceeded => Results.BadRequest(new ProblemDetails
            {
                Title = "Storage Quota Exceeded",
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