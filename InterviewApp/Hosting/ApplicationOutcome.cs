namespace InterviewApp.Hosting;

/// <summary>
/// Carries the exit code from <see cref="GreetingWorker"/> back to <c>Main</c>. Injected rather
/// than using <see cref="Environment.ExitCode"/> so tests can assert on it without touching
/// process-global state.
/// </summary>
internal sealed class ApplicationOutcome
{
    public int ExitCode { get; private set; } = ExitCodes.Success;

    public void Fail(int exitCode) => ExitCode = exitCode;
}

/// <summary>Process exit codes, following BSD <c>sysexits.h</c>.</summary>
internal static class ExitCodes
{
    public const int Success = 0;
    public const int UnexpectedError = 70;
    public const int InvalidConfiguration = 78;
}
