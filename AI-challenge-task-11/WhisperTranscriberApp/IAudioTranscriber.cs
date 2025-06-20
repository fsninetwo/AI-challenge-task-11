namespace WhisperTranscriberApp;

public interface IAudioTranscriber
{
    /// <summary>
    /// Transcribes the specified audio file and returns the text.
    /// </summary>
    /// <param name="filePath">Path to the audio file (e.g., .wav, .mp3).</param>
    /// <returns>The transcription text.</returns>
    Task<string> TranscribeAsync(string filePath);
} 