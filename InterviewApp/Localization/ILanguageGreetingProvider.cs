namespace InterviewApp.Localization;

/// <summary>
/// Supplies the greeting phrases for one language. Adding a language means adding one
/// implementation and registering it in <c>AddGreetingServices</c>.
/// </summary>
public interface ILanguageGreetingProvider
{
    /// <summary>Language name, matched case-insensitively against <c>Greeting:Language</c>.</summary>
    string Language { get; }

    /// <summary>Time-independent salutation, e.g. "Hello".</summary>
    string Salutation { get; }

    /// <summary>Greeting for the given part of the day, e.g. "Good morning".</summary>
    string GetTimeOfDayGreeting(DayPeriod period);
}
