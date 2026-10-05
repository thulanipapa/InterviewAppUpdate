using InterviewApp.Localization;
using InterviewApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InterviewApp.Tests;

public sealed class LanguageGreetingResolverTests
{
    [Theory]
    [InlineData("English", "Hello")]
    [InlineData("Afrikaans", "Hallo")]
    [InlineData("Zulu", "Sawubona")]
    public void Provider_IsTheOneForTheConfiguredLanguage(string language, string expectedSalutation)
    {
        var provider = TestDoubles.Resolver(language).Provider;

        Assert.Equal(language, provider.Language);
        Assert.Equal(expectedSalutation, provider.Salutation);
    }

    [Theory]
    [InlineData("afrikaans")]
    [InlineData("AFRIKAANS")]
    [InlineData("aFrIkAaNs")]
    public void Provider_MatchesLanguageCaseInsensitively(string language)
    {
        Assert.Equal("Afrikaans", TestDoubles.Resolver(language).Provider.Language);
    }

    [Theory]
    [InlineData("Klingon")]
    [InlineData("French")]
    [InlineData("   ")]
    public void Provider_FallsBackToEnglish_WhenLanguageIsNotSupported(string language)
    {
        Assert.Equal(LanguageGreetingResolver.FallbackLanguage, TestDoubles.Resolver(language).Provider.Language);
    }

    [Fact]
    public void Provider_FallsBackToEnglish_WhenLanguageIsNull()
    {
        var resolver = new LanguageGreetingResolver(
            TestDoubles.Providers(),
            Microsoft.Extensions.Options.Options.Create(new GreetingOptions { Language = null! }),
            NullLogger<LanguageGreetingResolver>.Instance);

        Assert.Equal(LanguageGreetingResolver.FallbackLanguage, resolver.Provider.Language);
    }

    [Fact]
    public void Constructor_Throws_WhenNeitherConfiguredNorFallbackLanguageIsRegistered()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => new LanguageGreetingResolver(
            [new ZuluGreetingProvider()],
            TestDoubles.Options("Klingon"),
            NullLogger<LanguageGreetingResolver>.Instance));

        Assert.Contains("Klingon", exception.Message, StringComparison.Ordinal);
        Assert.Contains(LanguageGreetingResolver.FallbackLanguage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_Throws_WhenTwoProvidersDeclareTheSameLanguage()
    {
        Assert.Throws<ArgumentException>(() => new LanguageGreetingResolver(
            [new EnglishGreetingProvider(), new EnglishGreetingProvider()],
            TestDoubles.Options("English"),
            NullLogger<LanguageGreetingResolver>.Instance));
    }

    [Fact]
    public void UnsupportedLanguage_IsWarnedAboutOnlyOnce()
    {
        var logger = new CountingLogger<LanguageGreetingResolver>();
        var resolver = new LanguageGreetingResolver(
            TestDoubles.Providers(),
            TestDoubles.Options("Klingon"),
            logger);

        _ = resolver.Provider;
        _ = resolver.Provider;

        Assert.Equal(1, logger.WarningCount);
    }

    [Theory]
    [MemberData(nameof(EveryLanguageAndPeriod))]
    public void EveryProvider_HasAGreetingForEveryDayPeriod(ILanguageGreetingProvider provider, DayPeriod period)
    {
        Assert.False(string.IsNullOrWhiteSpace(provider.GetTimeOfDayGreeting(period)));
    }

    public static TheoryData<ILanguageGreetingProvider, DayPeriod> EveryLanguageAndPeriod()
    {
        var data = new TheoryData<ILanguageGreetingProvider, DayPeriod>();

        foreach (var provider in TestDoubles.Providers())
        {
            foreach (var period in Enum.GetValues<DayPeriod>())
            {
                data.Add(provider, period);
            }
        }

        return data;
    }

    private sealed class CountingLogger<T> : ILogger<T>
    {
        public int WarningCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                WarningCount++;
            }
        }
    }
}
