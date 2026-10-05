using InterviewApp.Localization;
using InterviewApp.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace InterviewApp.Tests;

public sealed class TimeGreetingServiceTests
{
    [Theory]
    [InlineData(0, DayPeriod.Evening)]
    [InlineData(4, DayPeriod.Evening)]
    [InlineData(5, DayPeriod.Morning)]
    [InlineData(11, DayPeriod.Morning)]
    [InlineData(12, DayPeriod.Afternoon)]
    [InlineData(16, DayPeriod.Afternoon)]
    [InlineData(17, DayPeriod.Evening)]
    [InlineData(23, DayPeriod.Evening)]
    public void ToDayPeriod_MapsTheBoundaryHours(int hour, DayPeriod expected)
    {
        Assert.Equal(expected, TimeGreetingService.ToDayPeriod(TimeSpan.FromHours(hour)));
    }

    [Theory]
    [InlineData(8, "English", "Good morning")]
    [InlineData(8, "Afrikaans", "Goeiemôre")]
    [InlineData(8, "Zulu", "Ekuseni okuhle")]
    [InlineData(14, "English", "Good afternoon")]
    [InlineData(14, "Afrikaans", "Goeiemiddag")]
    [InlineData(14, "Zulu", "Emini emihle")]
    [InlineData(19, "English", "Good evening")]
    [InlineData(19, "Afrikaans", "Goeienaand")]
    [InlineData(19, "Zulu", "Kusihlwa okuhle")]
    [InlineData(22, "English", "Good evening")]
    [InlineData(22, "Afrikaans", "Goeienaand")]
    public void GetTimeGreeting_ReturnsLocalisedGreetingForTheCurrentHour(
        int hour,
        string language,
        string expectedGreeting)
    {
        Assert.Equal(expectedGreeting, CreateService(hour, language).GetTimeGreeting());
    }

    [Fact]
    public void GetTimeGreeting_UsesLocalTimeRatherThanUtc()
    {
        // 14:00 UTC is 20:00 in UTC+6: evening locally, afternoon in UTC.
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 4, 14, 0, 0, TimeSpan.Zero));
        timeProvider.SetLocalTimeZone(
            TimeZoneInfo.CreateCustomTimeZone("Test+6", TimeSpan.FromHours(6), "Test+6", "Test+6"));

        var service = new TimeGreetingService(
            timeProvider,
            TestDoubles.Resolver("English"),
            NullLogger<TimeGreetingService>.Instance);

        Assert.Equal("Good evening", service.GetTimeGreeting());
    }

    [Fact]
    public void GetTimeGreeting_FallsBackToEnglish_WhenLanguageIsNotSupported()
    {
        Assert.Equal("Good morning", CreateService(hour: 8, language: "Klingon").GetTimeGreeting());
    }

    private static TimeGreetingService CreateService(int hour, string language)
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 4, hour, 0, 0, TimeSpan.Zero));

        // Pinned to UTC so local time equals the hour under test on any machine.
        timeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);

        return new TimeGreetingService(
            timeProvider,
            TestDoubles.Resolver(language),
            NullLogger<TimeGreetingService>.Instance);
    }
}
