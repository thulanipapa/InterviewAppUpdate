# Solution Notes

Implementation of the tasks in [`ReadMe.md`](ReadMe.md). All 8 tasks are complete, including the
optional unit tests.

```bash
dotnet build InterviewApp.sln     # 0 warnings, 0 errors
dotnet test  InterviewApp.sln     # 85 tests, all passing
dotnet run   --project InterviewApp
# Hello! Good morning. Welcome to the interview app!
```

stdout carries only the greeting. All log output goes to stderr, so `InterviewApp > out.txt`
captures exactly one line.

---

## Task-to-code map

| # | Task | Where |
|---|------|-------|
| 1 | Multi-language greeting + fallback | `Localization/ILanguageGreetingProvider.cs`, the three `*GreetingProvider.cs`, `Localization/LanguageGreetingResolver.cs` |
| 2 | Logging | `GreetingService`, `LanguageGreetingResolver`, `GreetingWorker` |
| 3 | Configuration validation + graceful exit | `Models/GreetingOptions.cs`, `ServiceCollectionExtensions.AddGreetingOptions`, `Program.TryValidateConfiguration` |
| 4 | Time-based greeting | `Services/ITimeGreetingService.cs`, `Services/TimeGreetingService.cs` |
| 5 | Unit tests (optional) | `InterviewApp.Tests`: xUnit + NSubstitute + `FakeTimeProvider` |
| 6 | MediatR registration | `ServiceCollectionExtensions.AddGreetingMediator` |
| 7 | `GreetUserCommand` / `GreetUserHandler` | `Features/Greeting/GreetUser.cs`, dispatched from `Hosting/GreetingWorker.cs` |
| 8 | `GetTimeGreetingQuery` + combination | `Features/Greeting/GetTimeGreeting.cs`, combined in `GreetUserHandler` |

---

## Bugs found in the skeleton

**1. `GreetingOptions` was never bound.** `IOptions<GreetingOptions>.Value` returned `null` for
both properties, so the app printed a blank line and reported nothing. Regression test:
`CompositionRootTests.Configuration_BindsTheGreetingSection`.

**2. The process never exited.** `greetingService.Run()` was followed by `await host.RunAsync()`,
which blocked until Ctrl+C. The work now runs in a `BackgroundService` that calls
`StopApplication()` when it finishes.

**3. `appsettings.json` was not copied to the output directory.** `dotnet run` worked, but the built
binary did not. Fixed with `CopyToOutputDirectory`.

**4. Configuration discovery depended on the working directory.** The default content root is the
current directory. It is now pinned to `AppContext.BaseDirectory`.

The skeleton also re-added `appsettings.json` manually (the builder already loads it) and registered
stateless services as transient. Both are now gone or fixed: the services are singletons.

---

## Design decisions

### One `GetGreeting(string timeGreeting)` method (Tasks 4 and 8)

Task 4 says to inject `ITimeGreetingService` into `GreetingService`. Task 8 says to combine the time
greeting through MediatR. Doing both would resolve the time twice. `GreetUserHandler` settles this:
it gets the phrase via `GetTimeGreetingQuery` and passes it to `GreetingService.GetGreeting`. So
`GreetingService` takes the phrase as an argument and does not depend on `ITimeGreetingService` at
all. This is a deliberate departure from Task 4's literal wording.

### Languages are polymorphic, not a `switch`

Each language implements `ILanguageGreetingProvider` and supplies all of its own phrases (the
salutation and the time-of-day greetings). `LanguageGreetingResolver` picks one once, at start-up.
An unsupported language logs a single warning and falls back to English. Adding a language means one
new class and one registration line.

### There is no "night" greeting

"Good night" and "Goeienag" are farewells, not greetings. `DayPeriod` therefore has only morning
(05:00), afternoon (12:00) and evening (17:00 to 05:00). At 22:00 the app says "Good evening".

### Localisation: the simple version, on purpose

A full localisation design is clear enough. Every user-facing string (salutation, time phrases,
message, and the sentence pattern itself) would move to `.resx` resources read through
`IStringLocalizer<T>`. `Greeting:Language` would become a culture code (`en-ZA`, `af-ZA`,
`zu-ZA`), resolved by `CultureInfo` with its built-in fallback chain. Each culture would own a
composite format string, so word order can differ by language. Adding a language would then mean
adding a translation file, with no code change.

I did not build that. The brief asks for three languages and one configured message. A resource
pipeline would add more infrastructure than the feature it serves, and it still needs translators
the project does not have. One small class per language gives the same extension point (add a
class, register it) in about 20 lines. Moving to resources later would change only how a provider
gets its strings, not its callers.

### Time comes from `TimeProvider`

`TimeProvider.System` is used in production and `FakeTimeProvider` in tests, with the time zone
pinned. Every boundary is asserted, and no test depends on the wall clock.

### Validation runs before `RunAsync()`

If `RunAsync()` triggers `ValidateOnStart`, the host disposes the service provider on the way out,
and logging the failure then throws `ObjectDisposedException`. `Program` calls `IStartupValidator`
explicitly while the provider is still alive:

```
$ InterviewApp.exe --Greeting:Message=
fail: InterviewApp.Program[0]
      Invalid configuration: ... 'Greeting:Message' is required and must not be blank.
fail: InterviewApp.Program[0]
      Startup aborted because the 'Greeting' configuration section is invalid. Exiting with code 78.
```

Exit codes follow `sysexits.h`: `0` success, `70` unexpected failure, `78` configuration error. An
injected `ApplicationOutcome` carries the code from the worker to `Main`. `Environment.ExitCode`
would work too, but it is process-global, and tests would leak it into the test host.

### Logging

- **Each failure is logged once, at the boundary** (`GreetingWorker`). The service and the MediatR
  pipeline do not catch at all: a component either handles an error (and logs it) or lets it
  propagate, never both. `LoggingBehavior` only times successful requests, at Debug level.
- **The "service started" entry** is written in `GreetingService`'s constructor. The service is a
  singleton, so this fires once, when the service starts. Task 2's "message displayed" is logged
  once, at Information level.
- **The unsupported-language warning** is logged by `LanguageGreetingResolver`, the component that
  detects the condition, rather than under `ILogger<GreetingService>` as Task 2 literally says.

### MediatR

- `ISender`, not `IMediator`: the handlers only send.
- Each request lives in the same file as its handler. Handlers are thin, and domain rules stay in
  the services.
- `GreetUserCommand` is named as the brief asks, although it is a query: it returns data and changes
  nothing.
- `MediatR.Extensions.Microsoft.DependencyInjection` was folded into `MediatR` at v12, so only
  `MediatR` is referenced. It is pinned to 12.4.1, the last Apache-2.0 release.

---

## Known limitations

- **The output mixes languages.** Only the salutation and the time-of-day phrase are localised. The
  configured `Message` and the `"{salutation}! {time}. {message}"` pattern are not. With
  `Language=Zulu` the output is `Sawubona! Ekuseni okuhle. Welcome to the interview app!`. The brief
  supplies a single `Message`; the full fix is described under "Localisation" above.
- **The Zulu phrases have not been verified.** "Ekuseni okuhle", "Emini emihle" and "Kusihlwa
  okuhle" read like literal translations of the English. A native speaker should review them before
  anyone relies on them; the idiomatic form may simply be "Sawubona" at any hour.
- **"Good evening" covers 00:00 to 05:00.** It is a reasonable greeting at those hours, but it is
  a simplification.

---

## Testing

85 tests across eight suites:

| Suite | Focus |
|-------|-------|
| `LanguageGreetingResolverTests` | Language selection, case-insensitivity, fallback, null language, duplicate registration, single warning, every provider covers every period |
| `TimeGreetingServiceTests` | Period boundaries, localisation per language, local time vs UTC |
| `GreetingOutputTests` | The exact sentence per language and time of day through the real pipeline; no hour ever produces a farewell |
| `GreetingServiceTests` | Composition, fallback, rejection of a blank time greeting |
| `GreetingHandlerTests` | MediatR orchestration, cancellation propagation |
| `GreetingWorkerTests` | Dispatch, exit-code reporting, failure path, cancellation treated as shutdown, not failure |
| `CompositionRootTests` | Real container: binding, validation, graph validity, end-to-end `Send()` |
| `HostIntegrationTests` | Real host: runs, stops on its own, outcome survives provider disposal |

`HostIntegrationTests` exists because of a bug no unit test caught. Resolving `ApplicationOutcome`
after `RunAsync()` threw `ObjectDisposedException`, because the host disposes its provider on
success too. The app exited with an error while every unit test passed. Only running the real host
end to end reveals lifecycle and ordering bugs like this.

NSubstitute was chosen over Moq because Moq 4.20's SponsorLink telemetry is hard to justify in many
organisations.
