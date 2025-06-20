namespace WhisperTranscriberApp.Services.Summarization;

/// <summary>
/// Abstraction for text summarization services.
/// </summary>
public interface ISummarizer
{
    /// <summary>
    /// Produces a concise summary of the provided text.
    /// </summary>
    Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default);
} 