using InterviewApp.Localization;
using InterviewApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterviewApp.Services;

/// <inheritdoc cref="IGreetingService" />
public sealed class GreetingService : IGreetingService
{
    private readonly ILanguageGreetingProvider _provider;
    private readonly GreetingOptions _options;
    private readonly ILogger<GreetingService> _logger;

    public GreetingService(
        LanguageGreetingResolver languageResolver,
        IOptions<GreetingOptions> options,
        ILogger<GreetingService> logger)
    {
        _provider = languageResolver.Provider;
        _options = options.Value;
        _logger = logger;

        // A singleton, so this runs once, when the service is first resolved.
        _logger.LogInformation("GreetingService started; language is {Language}.", _provider.Language);
    }

    /// <inheritdoc />
    public string GetGreeting(string timeGreeting)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeGreeting);

        // Only the salutation and time phrase are localised; the sentence pattern and the
        // configured message are not. See SOLUTION.md.
        var greeting = $"{_provider.Salutation}! {timeGreeting}. {_options.Message}";

        _logger.LogInformation("Displaying greeting: {Greeting}", greeting);

        return greeting;
    }
}
