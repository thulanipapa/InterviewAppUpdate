using InterviewApp.DependencyInjection;
using InterviewApp.Features.Greeting;
using InterviewApp.Localization;
using InterviewApp.Models;
using InterviewApp.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InterviewApp.Tests;

/// <summary>Exercises the application's real registrations: binding, validation and MediatR dispatch.</summary>
public sealed class CompositionRootTests
{
    [Fact]
    public void Configuration_BindsTheGreetingSection()
    {
        using var provider = BuildProvider(
            ("Greeting:Message", "Welcome to the interview app!"),
            ("Greeting:Language", "Zulu"));

        var options = provider.GetRequiredService<IOptions<GreetingOptions>>().Value;

        Assert.Equal("Welcome to the interview app!", options.Message);
        Assert.Equal("Zulu", options.Language);
    }

    [Theory]
    [InlineData("", "English")]
    [InlineData("   ", "English")]
    [InlineData("Welcome!", "")]
    [InlineData("Welcome!", "   ")]
    public void Validation_Fails_WhenMessageOrLanguageIsBlank(string message, string language)
    {
        using var provider = BuildProvider(
            ("Greeting:Message", message),
            ("Greeting:Language", language));

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact]
    public void Validation_Fails_WhenTheGreetingSectionIsAbsentEntirely()
    {
        using var provider = BuildProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        Assert.Contains("Greeting:Message", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validation_Succeeds_ForTheShippedConfiguration()
    {
        using var provider = BuildProvider(
            ("Greeting:Message", "Welcome to the interview app!"),
            ("Greeting:Language", "English"));

        provider.GetRequiredService<IStartupValidator>().Validate();
    }

    [Fact]
    public void Container_ResolvesEveryGreetingService()
    {
        using var provider = BuildProvider(
            ("Greeting:Message", "Welcome!"),
            ("Greeting:Language", "English"));

        Assert.NotNull(provider.GetRequiredService<IGreetingService>());
        Assert.NotNull(provider.GetRequiredService<ITimeGreetingService>());
        Assert.NotNull(provider.GetRequiredService<LanguageGreetingResolver>());
        Assert.NotNull(provider.GetRequiredService<ISender>());
    }

    [Fact]
    public void Container_RegistersEverySupportedLanguageExactlyOnce()
    {
        using var provider = BuildProvider(
            ("Greeting:Message", "Welcome!"),
            ("Greeting:Language", "English"));

        var languages = provider.GetServices<ILanguageGreetingProvider>()
            .Select(languageProvider => languageProvider.Language)
            .ToArray();

        Assert.Equal(["English", "Afrikaans", "Zulu"], languages);
    }

    [Theory]
    [InlineData("English", "Hello!")]
    [InlineData("Afrikaans", "Hallo!")]
    [InlineData("Zulu", "Sawubona!")]
    public async Task GreetUserCommand_FlowsThroughMediatRToAGreeting(string language, string expectedPrefix)
    {
        using var provider = BuildProvider(
            ("Greeting:Message", "Welcome to the interview app!"),
            ("Greeting:Language", language));

        var greeting = await provider.GetRequiredService<ISender>()
            .Send(new GreetUserCommand(), CancellationToken.None);

        Assert.StartsWith(expectedPrefix, greeting, StringComparison.Ordinal);
        Assert.EndsWith("Welcome to the interview app!", greeting, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetTimeGreetingQuery_IsIndependentlyDispatchable()
    {
        using var provider = BuildProvider(
            ("Greeting:Message", "Welcome!"),
            ("Greeting:Language", "English"));

        var timeGreeting = await provider.GetRequiredService<ISender>()
            .Send(new GetTimeGreetingQuery(), CancellationToken.None);

        string[] englishTimeGreetings = ["Good morning", "Good afternoon", "Good evening"];

        Assert.Contains(timeGreeting, englishTimeGreetings);
    }

    private static ServiceProvider BuildProvider(params (string Key, string Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(setting =>
                new KeyValuePair<string, string?>(setting.Key, setting.Value)))
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddGreetingOptions(configuration);
        services.AddGreetingServices();
        services.AddGreetingMediator();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
