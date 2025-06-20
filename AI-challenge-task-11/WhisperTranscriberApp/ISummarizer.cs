namespace WhisperTranscriberApp;

/// <summary>
/// Abstraction for text summarization services.
/// </summary>
public interface ISummarizer
{
    /// <summary>
    /// Produces a concise summary of the provided text.
    /// </summary>
    /// <param name="text">The text to summarize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default);
} 