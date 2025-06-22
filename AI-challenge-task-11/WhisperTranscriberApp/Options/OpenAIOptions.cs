namespace WhisperTranscriberApp.Options;

/// <summary>
/// Configuration options for connecting to the OpenAI API.
/// </summary>
public sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? WhisperModel { get; set; }
    public string? ChatModel { get; set; }
    /// <summary>
    /// The temperature used for GPT chat completions. If null, each call will fall back to its own sensible default.
    /// </summary>
    public double? Temperature { get; set; }
} 