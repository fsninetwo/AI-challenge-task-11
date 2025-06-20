namespace WhisperTranscriberApp;

/// <summary>
/// Configuration options for connecting to the OpenAI API.
/// Values are bound from the <c>OpenAI</c> section in <c>appsettings.json</c>.
/// </summary>
public sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// Base URL for the OpenAI platform (default "https://api.openai.com/").
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Secret API key used for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Whisper model name (default "whisper-1").
    /// </summary>
    public string? WhisperModel { get; set; } = "whisper-1";
} 