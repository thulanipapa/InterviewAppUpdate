using InterviewApp.DependencyInjection;
using InterviewApp.Hosting;
using InterviewApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterviewApp;

/// <summary>Application entry point and composition root.</summary>
internal sealed class Program
{
    private Program()
    {
    }

    private static async Task<int> Main(string[] args)
    {
        IHost host;

        try
        {
            host = BuildHost(args);
        }
        catch (Exception exception)
        {
            // No logger exists until the host is built.
            await Console.Error
                .WriteLineAsync($"Fatal: the application could not be configured. {exception.Message}")
                .ConfigureAwait(false);

            return ExitCodes.InvalidConfiguration;
        }

        using (host)
        {
            // Resolve before RunAsync: the host disposes its service provider when RunAsync completes.
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var outcome = host.Services.GetRequiredService<ApplicationOutcome>();

            if (!TryValidateConfiguration(host.Services, logger))
            {
                return ExitCodes.InvalidConfiguration;
            }

            try
            {
                await host.RunAsync().ConfigureAwait(false);

                return outcome.ExitCode;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "The application terminated unexpectedly.");

                return ExitCodes.UnexpectedError;
            }
        }
    }

    /// <summary>
    /// Runs the start-up validators while the service provider is still alive, so a failure can be
    /// logged instead of escaping from <c>RunAsync</c> as an exception.
    /// </summary>
    private static bool TryValidateConfiguration(IServiceProvider services, ILogger logger)
    {
        try
        {
            services.GetRequiredService<IStartupValidator>().Validate();

            return true;
        }
        catch (OptionsValidationException exception)
        {
            LogInvalidConfiguration(logger, [exception]);

            return false;
        }
        catch (AggregateException exception)
        {
            // Raised when more than one options type fails validation.
            LogInvalidConfiguration(
                logger,
                exception.Flatten().InnerExceptions.OfType<OptionsValidationException>());

            return false;
        }
    }

    private static void LogInvalidConfiguration(
        ILogger logger,
        IEnumerable<OptionsValidationException> exceptions)
    {
        foreach (var failure in exceptions.SelectMany(exception => exception.Failures))
        {
            logger.LogError("Invalid configuration: {ValidationFailure}", failure);
        }

        logger.LogError(
            "Startup aborted because the '{SectionName}' configuration section is invalid. Exiting with code {ExitCode}.",
            GreetingOptions.SectionName,
            ExitCodes.InvalidConfiguration);
    }

    private static IHost BuildHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,

            // Find appsettings.json next to the executable, whatever the working directory.
            ContentRootPath = AppContext.BaseDirectory
        });

        // stdout carries only the greeting; all diagnostics go to stderr.
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

        builder.Services
            .AddGreetingOptions(builder.Configuration)
            .AddGreetingServices()
            .AddGreetingMediator()
            .AddGreetingWorker();

        return builder.Build();
    }
}
