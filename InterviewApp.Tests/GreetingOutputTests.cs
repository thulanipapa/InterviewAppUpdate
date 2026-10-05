using InterviewApp.DependencyInjection;
using InterviewApp.Features.Greeting;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace InterviewApp.Tests;

/// <summary>
/// Pins the exact sentence a user sees, per language and time of day, through the real pipeline.
/// The other suites check how the greeting is built; this one checks what it says.
/// </summary>
public sealed class GreetingOutputTests
{
    private const string Message = "Welcome to the interview app!";

    // The English message in the Afrikaans and Zulu rows is a known limitation (see SOLUTION.md):
    // only the salutation and the time-of-day phrase are localised.
    [Theory]
    [InlineData("English", 8, "Hello! Good morning. Welcome to the interview app!")]
    [InlineData("English", 14, "Hello! Good afternoon. Welcome to the interview app!")]
    [InlineData("English", 22, "Hello! Good evening. Welcome to the interview app!")]
    [InlineData("Afrikaans", 8, "Hallo! Goeiemôre. Welcome to the interview app!")]
    [InlineData("Afrikaans", 14, "Hallo! Goeiemiddag. Welcome to the interview app!")]
    [InlineData("Afrikaans", 22, "Hallo! Goeienaand. Welcome to the interview app!")]
    [InlineData("Zulu", 8, "Sawubona! Ekuseni okuhle. Welcome to the interview app!")]
    [InlineData("Zulu", 14, "Sawubona! Emini emihle. Welcome to the interview app!")]
    [InlineData("Zulu", 22, "Sawubona! Kusihlwa okuhle. Welcome to the interview app!")]
    public async Task Greeting_ReadsAsExpected(string language, int hour, string expected)
    {
        Assert.Equal(expected, await GreetAsync(language, hour));
    }

    [Theory]
    [InlineData("English")]
    [InlineData("Afrikaans")]
    [InlineData("Zulu")]
    public async Task Greeting_IsNeverAFarewell(string language)
    {
        // "Good night", "Goeienag" and "Ubusuku obuhle" are said when leaving, not arriving.
        string[] farewells = ["Good night", "Goeienag", "Ubusuku"];

        for (var hour = 0; hour < 24; hour++)
        {
            var greeting = await GreetAsync(language, hour);

            Assert.DoesNotContain(farewells, farewell => greeting.Contains(farewell, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static async Task<string> GreetAsync(string language, int hour)
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 4, hour, 0, 0, TimeSpan.Zero));
        timeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Greeting:Message"] = Message,
                ["Greeting:Language"] = language
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Registered first, so AddGreetingServices' TryAddSingleton keeps the fake clock.
        services.AddSingleton<TimeProvider>(timeProvider);
        services.AddGreetingOptions(configuration).AddGreetingServices().AddGreetingMediator();

        await using var provider = services.BuildServiceProvider();

        return await provider.GetRequiredService<ISender>().Send(new GreetUserCommand());
    }
}
