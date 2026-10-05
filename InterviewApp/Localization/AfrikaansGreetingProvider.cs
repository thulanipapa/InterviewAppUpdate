namespace InterviewApp.Localization;

/// <summary>Afrikaans greeting phrases.</summary>
public sealed class AfrikaansGreetingProvider : ILanguageGreetingProvider
{
    /// <inheritdoc />
    public string Language => "Afrikaans";

    /// <inheritdoc />
    public string Salutation => "Hallo";

    /// <inheritdoc />
    public string GetTimeOfDayGreeting(DayPeriod period) => period switch
    {
        DayPeriod.Morning => "Goeiemôre",
        DayPeriod.Afternoon => "Goeiemiddag",
        DayPeriod.Evening => "Goeienaand",
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Unrecognised day period.")
    };
}
