using InterviewApp.DependencyInjection;
using InterviewApp.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InterviewApp.Tests;

/// <summary>Runs the real host end to end: configuration, DI, MediatR, the worker and shutdown.</summary>
public sealed class HostIntegrationTests
{
    // The timeout makes a worker that never stops the host fail rather than hang the suite.
    [Fact(Timeout = 30_000)]
    public async Task Host_RunsTheWorker_ThenTerminatesOnItsOwn()
    {
        using var host = BuildHost(("Greeting:Message", "Welcome!"), ("Greeting:Language", "Zulu"));

        var outcome = host.Services.GetRequiredService<ApplicationOutcome>();

        await host.RunAsync();

        Assert.Equal(ExitCodes.Success, outcome.ExitCode);
    }

    [Fact(Timeout = 30_000)]
    public async Task Host_ReportsTheOutcome_AfterTheProviderIsDisposed()
    {
        using var host = BuildHost(("Greeting:Message", "Welcome!"), ("Greeting:Language", "English"));

        // Must be resolved before RunAsync, as Program does.
        var outcome = host.Services.GetRequiredService<ApplicationOutcome>();

        await host.RunAsync();

        Assert.Equal(ExitCodes.Success, outcome.ExitCode);
        Assert.Throws<ObjectDisposedException>(
            () => host.Services.GetRequiredService<ApplicationOutcome>());
    }

    private static IHost BuildHost(params (string Key, string Value)[] settings)
    {
        var builder = Host.CreateApplicationBuilder();

        // Added last so it overrides any appsettings.json in the test output directory.
        builder.Configuration.AddInMemoryCollection(settings.Select(setting =>
            new KeyValuePair<string, string?>(setting.Key, setting.Value)));

        builder.Services
            .AddGreetingOptions(builder.Configuration)
            .AddGreetingServices()
            .AddGreetingMediator()
            .AddGreetingWorker();

        return builder.Build();
    }
}
