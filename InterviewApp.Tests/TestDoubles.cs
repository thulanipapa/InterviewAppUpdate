using InterviewApp.Localization;
using InterviewApp.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InterviewApp.Tests;

internal static class TestDoubles
{
    public static IOptions<GreetingOptions> Options(string language, string message = "Welcome!") =>
        Microsoft.Extensions.Options.Options.Create(new GreetingOptions
        {
            Language = language,
            Message = message
        });

    /// <summary>A real resolver over the production providers, which are plain value lookups.</summary>
    public static LanguageGreetingResolver Resolver(string language) =>
        new(Providers(), Options(language), NullLogger<LanguageGreetingResolver>.Instance);

    public static ILanguageGreetingProvider[] Providers() =>
    [
        new EnglishGreetingProvider(),
        new AfrikaansGreetingProvider(),
        new ZuluGreetingProvider()
    ];
}
