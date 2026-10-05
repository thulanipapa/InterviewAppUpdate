using InterviewApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterviewApp.Localization;

/// <summary>
/// Picks the provider for the configured language, falling back to English with a warning when
/// the language is not supported. Resolved once, so a misconfiguration produces one warning.
/// </summary>
public sealed class LanguageGreetingResolver
{
    /// <summary>Language used when the configured one has no registered provider.</summary>
    public const string FallbackLanguage = "English";

    /// <exception cref="ArgumentException">Two providers declare the same language.</exception>
    /// <exception cref="InvalidOperationException">Neither the configured nor the fallback language is registered.</exception>
    public LanguageGreetingResolver(
        IEnumerable<ILanguageGreetingProvider> providers,
        IOptions<GreetingOptions> options,
        ILogger<LanguageGreetingResolver> logger)
    {
        var providersByLanguage = providers.ToDictionary(
            provider => provider.Language,
            StringComparer.OrdinalIgnoreCase);

        var configuredLanguage = options.Value.Language ?? string.Empty;

        if (providersByLanguage.TryGetValue(configuredLanguage, out var provider))
        {
            Provider = provider;
            return;
        }

        logger.LogWarning(
            "Language {ConfiguredLanguage} is not supported; falling back to {FallbackLanguage}. Supported languages: {SupportedLanguages}.",
            configuredLanguage,
            FallbackLanguage,
            string.Join(", ", providersByLanguage.Keys.Order(StringComparer.OrdinalIgnoreCase)));

        Provider = providersByLanguage.TryGetValue(FallbackLanguage, out var fallbackProvider)
            ? fallbackProvider
            : throw new InvalidOperationException(
                $"No {nameof(ILanguageGreetingProvider)} is registered for the configured language " +
                $"'{configuredLanguage}' or for the fallback language '{FallbackLanguage}'.");
    }

    /// <summary>The provider for the configured language, or the fallback provider.</summary>
    public ILanguageGreetingProvider Provider { get; }
}
