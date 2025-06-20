namespace WhisperTranscriberApp.Services.Transcription;

/// <summary>
/// Abstraction for audio transcription services.
/// </summary>
public interface IAudioTranscriber
{
    /// <summary>
    /// Transcribes the specified audio file and returns the text.
    /// </summary>
    /// <param name="filePath">Path to the audio file (e.g., .wav, .mp3).</param>
    Task<string> TranscribeAsync(string filePath);
} 