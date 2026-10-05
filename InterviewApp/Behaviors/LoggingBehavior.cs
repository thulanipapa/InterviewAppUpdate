using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InterviewApp.Behaviors;

/// <summary>
/// MediatR pipeline behaviour that times every successful request. Failures propagate untouched;
/// only the code that handles them (the worker) logs them.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next().ConfigureAwait(false);

        logger.LogDebug("Handled {RequestName} in {ElapsedMilliseconds} ms.",
            typeof(TRequest).Name,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
