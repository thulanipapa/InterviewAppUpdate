using InterviewApp.Behaviors;
using InterviewApp.Hosting;
using InterviewApp.Localization;
using InterviewApp.Models;
using InterviewApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InterviewApp.DependencyInjection;

/// <summary>
/// Registrations shared by <c>Program</c> and the tests, so both build the same container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Binds <see cref="GreetingOptions"/> and validates it at start-up.</summary>
    public static IServiceCollection AddGreetingOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<GreetingOptions>()
            .Bind(configuration.GetSection(GreetingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>Registers the greeting services and one provider per supported language.</summary>
    public static IServiceCollection AddGreetingServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<ILanguageGreetingProvider, EnglishGreetingProvider>();
        services.AddSingleton<ILanguageGreetingProvider, AfrikaansGreetingProvider>();
        services.AddSingleton<ILanguageGreetingProvider, ZuluGreetingProvider>();

        services.AddSingleton<LanguageGreetingResolver>();
        services.AddSingleton<ITimeGreetingService, TimeGreetingService>();
        services.AddSingleton<IGreetingService, GreetingService>();

        return services;
    }

    /// <summary>Registers MediatR, its handlers and the logging pipeline behaviour.</summary>
    public static IServiceCollection AddGreetingMediator(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<Program>();
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services;
    }

    /// <summary>Registers the hosted worker and the outcome it reports back to <c>Main</c>.</summary>
    public static IServiceCollection AddGreetingWorker(this IServiceCollection services)
    {
        services.AddSingleton<ApplicationOutcome>();
        services.AddHostedService<GreetingWorker>();

        return services;
    }
}
