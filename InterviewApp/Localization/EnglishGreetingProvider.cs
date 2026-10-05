namespace InterviewApp.Localization;

/// <summary>English greeting phrases. Also the fallback language.</summary>
public sealed class EnglishGreetingProvider : ILanguageGreetingProvider
{
    /// <inheritdoc />
    public string Language => "English";

    /// <inheritdoc />
    public string Salutation => "Hello";

    /// <inheritdoc />
    public string GetTimeOfDayGreeting(DayPeriod period) => period switch
    {
        DayPeriod.Morning => "Good morning",
        DayPeriod.Afternoon => "Good afternoon",
        DayPeriod.Evening => "Good evening",
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Unrecognised day period.")
    };
}
