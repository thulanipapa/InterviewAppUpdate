using InterviewApp.Services;
using MediatR;

namespace InterviewApp.Features.Greeting;

/// <summary>Asks for the complete greeting to show the user.</summary>
public sealed record GreetUserCommand : IRequest<string>;

/// <summary>
/// Handles <see cref="GreetUserCommand"/>: gets the time-of-day phrase through
/// <see cref="GetTimeGreetingQuery"/>, then has <see cref="IGreetingService"/> compose the greeting.
/// </summary>
public sealed class GreetUserHandler(ISender sender, IGreetingService greetingService)
    : IRequestHandler<GreetUserCommand, string>
{
    /// <inheritdoc />
    public async Task<string> Handle(GreetUserCommand request, CancellationToken cancellationToken)
    {
        var timeGreeting = await sender
            .Send(new GetTimeGreetingQuery(), cancellationToken)
            .ConfigureAwait(false);

        return greetingService.GetGreeting(timeGreeting);
    }
}
