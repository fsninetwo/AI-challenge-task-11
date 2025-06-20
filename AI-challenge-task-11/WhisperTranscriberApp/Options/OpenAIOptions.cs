namespace WhisperTranscriberApp.Options;

/// <summary>
/// Configuration options for connecting to the OpenAI API.
/// </summary>
public sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? WhisperModel { get; set; } = "whisper-1";
    public string? ChatModel { get; set; } = "gpt-3.5-turbo";
} 