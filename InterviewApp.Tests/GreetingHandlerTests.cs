using InterviewApp.Features.Greeting;
using InterviewApp.Services;
using MediatR;
using NSubstitute;

namespace InterviewApp.Tests;

public sealed class GreetingHandlerTests
{
    [Fact]
    public async Task GreetUserHandler_ObtainsTheTimeGreetingThroughMediatR()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetTimeGreetingQuery>(), Arg.Any<CancellationToken>())
            .Returns("Good morning");

        var greetingService = Substitute.For<IGreetingService>();
        greetingService.GetGreeting("Good morning").Returns("Hello! Good morning. Welcome!");

        var handler = new GreetUserHandler(sender, greetingService);

        var greeting = await handler.Handle(new GreetUserCommand(), CancellationToken.None);

        Assert.Equal("Hello! Good morning. Welcome!", greeting);
        await sender.Received(1).Send(Arg.Any<GetTimeGreetingQuery>(), Arg.Any<CancellationToken>());
        greetingService.Received(1).GetGreeting("Good morning");
    }

    [Fact]
    public async Task GreetUserHandler_PropagatesTheCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();

        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetTimeGreetingQuery>(), Arg.Any<CancellationToken>())
            .Returns("Good morning");

        var handler = new GreetUserHandler(sender, Substitute.For<IGreetingService>());

        await handler.Handle(new GreetUserCommand(), cancellation.Token);

        await sender.Received(1).Send(Arg.Any<GetTimeGreetingQuery>(), cancellation.Token);
    }

    [Fact]
    public async Task GetTimeGreetingHandler_ReturnsTheTimeGreetingFromTheService()
    {
        var timeGreetingService = Substitute.For<ITimeGreetingService>();
        timeGreetingService.GetTimeGreeting().Returns("Goeiemiddag");

        var handler = new GetTimeGreetingHandler(timeGreetingService);

        var greeting = await handler.Handle(new GetTimeGreetingQuery(), CancellationToken.None);

        Assert.Equal("Goeiemiddag", greeting);
    }

    [Fact]
    public async Task GetTimeGreetingHandler_HonoursAnAlreadyCancelledToken()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var handler = new GetTimeGreetingHandler(Substitute.For<ITimeGreetingService>());

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(new GetTimeGreetingQuery(), cancellation.Token));
    }
}
