using System.Threading;

namespace WhisperTranscriberApp;

/// <summary>
/// Orchestrates the overall flow of the console application: parses arguments, validates the audio file, and outputs the transcript.
/// Placing this logic in its own class keeps <c>Program.cs</c> minimal and improves testability.
/// </summary>
public class TranscriptionRunner
{
    private readonly IAudioTranscriber _transcriber;

    public TranscriptionRunner(IAudioTranscriber transcriber)
    {
        _transcriber = transcriber;
    }

    /// <summary>
    /// Executes the transcription workflow.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var filePath = args.Length > 0 ? args[0] : "CAR0004.mp3";

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"File '{filePath}' not found.");
            return;
        }

        try
        {
            string transcript = await _transcriber.TranscribeAsync(filePath);
            Console.WriteLine("----- Transcript -----");
            Console.WriteLine(transcript);
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operation was cancelled.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error during transcription: {ex.Message}");
        }
    }
} 