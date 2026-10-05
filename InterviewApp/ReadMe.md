# InterviewApp

This is a starter .NET Core console application designed for interview purposes. The goal is to evaluate your understanding of:

- .NET Core Console Application structure
- Dependency Injection (DI)
- Configuration via `appsettings.json`
- MediatR for decoupled request handling
- Debugging
- Clean code and extensibility principles

---

## 🧠 Tasks

Please complete the following tasks. You may use any libraries or patterns you are comfortable with, but aim for clarity, maintainability, and testability.

### 1. Extend the Greeting Service

- Modify `GreetingService` to support multiple languages (e.g., English, Afrikaans, Zulu).
- Use the `Language` property from `appsettings.json` to determine which greeting to display.
- Add a fallback if the language is not supported.

### 2. Add Logging

- Inject `ILogger<GreetingService>` and log:
    - When the service starts
    - What message is displayed
    - Any errors or unsupported languages
  
### 3. Validate Configuration

- Ensure that the `Message` and `Language` values in `appsettings.json` are not null or empty.
- If validation fails, log an error and exit gracefully.

### 4. Add a Time-Based Greeting Option

- Add a new service `ITimeGreetingService` that returns a greeting based on the current time (e.g., "Good morning", "Good afternoon").
- Inject this service into `GreetingService` and combine it with the configured message.

### 5. Unit Tests (Optional)

- Write unit tests for `GreetingService` using a testing framework of your choice (e.g., xUnit, NUnit).
- Mock dependencies where appropriate.

---

## 🧭 Advanced Tasks: MediatR Integration

These tasks are designed to assess your familiarity with the MediatR library and how to use it to decouple application logic.

### 6. Integrate MediatR

- Add the MediatR NuGet package to the project.
- Register MediatR in the DI container.

### 7. Create a Greeting Request

- Define a `GreetUserCommand` (or `Query`) that encapsulates the greeting logic.
- Implement a `GreetUserHandler` that handles the command and returns the greeting message.
- Replace the direct call to `GreetingService.Run()` with a MediatR `Send()` call.

### 8. Add a Time-Based Greeting Request
- Create a separate `GetTimeGreetingQuery` and handler.
- Combine the result with the configured greeting message using MediatR.

---

## 🧰 Suggested NuGet Packages

```bash
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
```

---

## 🛠️ Setup Instructions

1. Ensure you have .NET 8 SDK or later installed
2. Run the application:

Note: You may use any IDE of the following: Visual Studio, Visual Studio Code, or Jetbrains Rider

```bash
dotnet run
```