using InterviewApp.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace InterviewApp.Tests;

public sealed class GreetingServiceTests
{
    [Theory]
    [InlineData("English", "Good morning", "Hello! Good morning. Welcome!")]
    [InlineData("Afrikaans", "Goeiemôre", "Hallo! Goeiemôre. Welcome!")]
    [InlineData("Zulu", "Emini emihle", "Sawubona! Emini emihle. Welcome!")]
    public void GetGreeting_CombinesSalutationTimeGreetingAndConfiguredMessage(
        string language,
        string timeGreeting,
        string expected)
    {
        var service = CreateService(language);

        Assert.Equal(expected, service.GetGreeting(timeGreeting));
    }

    [Fact]
    public void GetGreeting_UsesTheConfiguredMessageVerbatim()
    {
        var service = CreateService("English", message: "Interview time.");

        Assert.EndsWith("Interview time.", service.GetGreeting("Good morning"), StringComparison.Ordinal);
    }

    [Fact]
    public void GetGreeting_FallsBackToEnglishSalutation_WhenLanguageIsNotSupported()
    {
        var service = CreateService("Klingon");

        Assert.Equal("Hello! Good morning. Welcome!", service.GetGreeting("Good morning"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetGreeting_Throws_WhenTimeGreetingIsBlank(string? timeGreeting)
    {
        var service = CreateService("English");

        // ThrowsAny: null raises the ArgumentNullException subclass.
        Assert.ThrowsAny<ArgumentException>(() => service.GetGreeting(timeGreeting!));
    }

    private static GreetingService CreateService(string language, string message = "Welcome!") =>
        new(
            TestDoubles.Resolver(language),
            TestDoubles.Options(language, message),
            NullLogger<GreetingService>.Instance);
}
