using JobTracker.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Errors;
using Shared.Results;

namespace JobTracker.Application.Commands.Interviews;

public record DeleteInterviewCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}

public class DeleteInterviewCommandHandler : IRequestHandler<DeleteInterviewCommand, Result<Unit>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly ILogger<DeleteInterviewCommandHandler> _logger;

    public DeleteInterviewCommandHandler(
        IInterviewRepository interviewRepository,
        ILogger<DeleteInterviewCommandHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteInterviewCommand request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (interview == null)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.InterviewNotFound,
                "Interview not found"));
        }

        // Verify the interview belongs to the user
        if (interview.Application.UserId != request.UserId)
        {
            return Result.Failure<Unit>(new Error(
                ErrorCodes.Forbidden,
                "You don't have permission to delete this interview"));
        }

        await _interviewRepository.DeleteAsync(interview, cancellationToken);

        _logger.LogInformation(
            "Interview deleted successfully. Id: {InterviewId}, ApplicationId: {ApplicationId}",
            interview.Id, interview.ApplicationId);

        return Result.Success(Unit.Value);
    }
}