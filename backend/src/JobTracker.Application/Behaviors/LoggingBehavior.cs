using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Results;

namespace JobTracker.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName} {@Request}", requestName, request);

        try
        {
            var response = await next();
            
            stopwatch.Stop();

            if (response.IsSuccess)
            {
                _logger.LogInformation(
                    "Handled {RequestName} successfully in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Handled {RequestName} with error {Error} in {ElapsedMs}ms",
                    requestName,
                    response.Error,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Error handling {RequestName} after {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}