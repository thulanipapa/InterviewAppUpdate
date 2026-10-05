using InterviewApp.Features.Greeting;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InterviewApp.Hosting;

/// <summary>
/// Sends <see cref="GreetUserCommand"/>, writes the greeting to stdout, then stops the host so the
/// process exits on its own.
/// </summary>
internal sealed class GreetingWorker(
    ISender sender,
    IHostApplicationLifetime lifetime,
    ApplicationOutcome outcome,
    ILogger<GreetingWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var greeting = await sender
                .Send(new GreetUserCommand(), stoppingToken)
                .ConfigureAwait(false);

            Console.WriteLine(greeting);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Greeting was cancelled because the application is shutting down.");
        }
        catch (Exception exception)
        {
            // The application boundary: the only place a failure is logged.
            logger.LogError(exception, "Failed to produce the greeting; the application will exit.");
            outcome.Fail(ExitCodes.UnexpectedError);
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
