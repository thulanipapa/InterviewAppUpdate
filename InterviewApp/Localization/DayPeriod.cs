namespace InterviewApp.Localization;

/// <summary>Part of the day a greeting is phrased for.</summary>
/// <remarks>
/// There is deliberately no "night": "Good night" is a farewell, so late hours greet as evening.
/// </remarks>
public enum DayPeriod
{
    /// <summary>05:00 up to 12:00.</summary>
    Morning,

    /// <summary>12:00 up to 17:00.</summary>
    Afternoon,

    /// <summary>17:00 up to 05:00.</summary>
    Evening
}
