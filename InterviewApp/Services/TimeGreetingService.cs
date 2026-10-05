using InterviewApp.Localization;
using Microsoft.Extensions.Logging;

namespace InterviewApp.Services;

/// <inheritdoc cref="ITimeGreetingService" />
public sealed class TimeGreetingService(
    TimeProvider timeProvider,
    LanguageGreetingResolver languageResolver,
    ILogger<TimeGreetingService> logger) : ITimeGreetingService
{
    /// <inheritdoc />
    public string GetTimeGreeting()
    {
        var localNow = timeProvider.GetLocalNow();
        var period = ToDayPeriod(localNow.TimeOfDay);
        var greeting = languageResolver.Provider.GetTimeOfDayGreeting(period);

        logger.LogDebug(
            "Local time {LocalTime:HH:mm} falls in {DayPeriod}; time greeting is \"{TimeGreeting}\".",
            localNow,
            period,
            greeting);

        return greeting;
    }

    internal static DayPeriod ToDayPeriod(TimeSpan localTimeOfDay) => localTimeOfDay.Hours switch
    {
        >= 5 and < 12 => DayPeriod.Morning,
        >= 12 and < 17 => DayPeriod.Afternoon,
        _ => DayPeriod.Evening
    };
}
