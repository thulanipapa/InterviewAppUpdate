using InterviewApp.Features.Greeting;
using InterviewApp.Hosting;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace InterviewApp.Tests;

public sealed class GreetingWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_DispatchesGreetUserCommandAndStopsTheApplication()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GreetUserCommand>(), Arg.Any<CancellationToken>())
            .Returns("Hello! Good morning. Welcome!");

        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var outcome = new ApplicationOutcome();

        await RunWorkerAsync(sender, lifetime, outcome, CancellationToken.None);

        await sender.Received(1).Send(Arg.Any<GreetUserCommand>(), Arg.Any<CancellationToken>());
        Assert.Equal(ExitCodes.Success, outcome.ExitCode);
        lifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task ExecuteAsync_RecordsAFailureExitCode_WhenTheGreetingThrows()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GreetUserCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("greeting unavailable"));

        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var outcome = new ApplicationOutcome();

        await RunWorkerAsync(sender, lifetime, outcome, CancellationToken.None);

        Assert.Equal(ExitCodes.UnexpectedError, outcome.ExitCode);
        lifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task ExecuteAsync_TreatsCancellationAsShutdownRatherThanFailure()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GreetUserCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        var outcome = new ApplicationOutcome();

        await RunWorkerAsync(sender, Substitute.For<IHostApplicationLifetime>(), outcome, cancellation.Token);

        Assert.Equal(ExitCodes.Success, outcome.ExitCode);
    }

    private static async Task RunWorkerAsync(
        ISender sender,
        IHostApplicationLifetime lifetime,
        ApplicationOutcome outcome,
        CancellationToken cancellationToken)
    {
        using var worker = new GreetingWorker(sender, lifetime, outcome, NullLogger<GreetingWorker>.Instance);

        await worker.StartAsync(cancellationToken);

        // StartAsync returns as soon as ExecuteAsync yields; wait for the work itself.
        await worker.ExecuteTask!;
    }
}
