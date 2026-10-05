using System.ComponentModel.DataAnnotations;

namespace InterviewApp.Models;

/// <summary>The <c>Greeting</c> section of <c>appsettings.json</c>, validated at start-up.</summary>
public sealed class GreetingOptions
{
    /// <summary>Name of the configuration section this type binds to.</summary>
    public const string SectionName = "Greeting";

    /// <summary>Message appended to the greeting, e.g. "Welcome to the interview app!".</summary>
    [Required(AllowEmptyStrings = false,
        ErrorMessage = "Configuration value 'Greeting:Message' is required and must not be blank.")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Language the greeting is rendered in, e.g. "Zulu".</summary>
    [Required(AllowEmptyStrings = false,
        ErrorMessage = "Configuration value 'Greeting:Language' is required and must not be blank.")]
    public string Language { get; set; } = string.Empty;
}
