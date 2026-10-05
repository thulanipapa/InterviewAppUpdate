namespace InterviewApp.Services;

/// <summary>Produces a greeting for the current time of day, in the configured language.</summary>
public interface ITimeGreetingService
{
    /// <summary>Returns the localised time-of-day greeting, e.g. "Good morning".</summary>
    string GetTimeGreeting();
}
