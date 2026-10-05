namespace InterviewApp.Services;

/// <summary>Composes the greeting that the application displays.</summary>
public interface IGreetingService
{
    /// <summary>
    /// Combines the configured language's salutation, <paramref name="timeGreeting"/> and the
    /// configured message.
    /// </summary>
    /// <param name="timeGreeting">Localised time-of-day phrase, e.g. "Good morning".</param>
    /// <exception cref="ArgumentException"><paramref name="timeGreeting"/> is null or blank.</exception>
    string GetGreeting(string timeGreeting);
}
