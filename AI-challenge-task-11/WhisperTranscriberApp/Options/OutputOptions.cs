namespace WhisperTranscriberApp.Options;

/// <summary>
/// Configuration options for controlling where result files are saved.
/// </summary>
public sealed class OutputOptions
{
    public const string SectionName = "Output";

    /// <summary>
    /// Directory where transcript/summary/analytics markdown files should be written.
    /// Can be absolute or relative. Relative paths are resolved against the application base directory.
    /// Defaults to "Transcripts" under the base directory.
    /// </summary>
    public string? Directory { get; set; } = "Transcripts";
} 