using InterviewApp.Services;
using MediatR;

namespace InterviewApp.Features.Greeting;

/// <summary>Asks for the time-of-day greeting in the configured language.</summary>
public sealed record GetTimeGreetingQuery : IRequest<string>;

/// <summary>Handles <see cref="GetTimeGreetingQuery"/> by delegating to <see cref="ITimeGreetingService"/>.</summary>
public sealed class GetTimeGreetingHandler(ITimeGreetingService timeGreetingService)
    : IRequestHandler<GetTimeGreetingQuery, string>
{
    /// <inheritdoc />
    public Task<string> Handle(GetTimeGreetingQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(timeGreetingService.GetTimeGreeting());
    }
}
