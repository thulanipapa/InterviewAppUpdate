namespace InterviewApp.Localization;

/// <summary>isiZulu greeting phrases. Not yet reviewed by a native speaker; see SOLUTION.md.</summary>
public sealed class ZuluGreetingProvider : ILanguageGreetingProvider
{
    /// <inheritdoc />
    public string Language => "Zulu";

    /// <inheritdoc />
    public string Salutation => "Sawubona";

    /// <inheritdoc />
    public string GetTimeOfDayGreeting(DayPeriod period) => period switch
    {
        DayPeriod.Morning => "Ekuseni okuhle",
        DayPeriod.Afternoon => "Emini emihle",
        DayPeriod.Evening => "Kusihlwa okuhle",
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Unrecognised day period.")
    };
}
