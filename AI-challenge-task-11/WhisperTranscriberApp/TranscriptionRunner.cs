using System.Threading;

namespace WhisperTranscriberApp;

/// <summary>
/// Orchestrates the overall flow of the console application: parses arguments, validates the audio file, and outputs the transcript.
/// Placing this logic in its own class keeps <c>Program.cs</c> minimal and improves testability.
/// </summary>
public class TranscriptionRunner
{
    private readonly IAudioTranscriber _transcriber;
    private readonly ISummarizer _summarizer;

    public TranscriptionRunner(IAudioTranscriber transcriber, ISummarizer summarizer)
    {
        _transcriber = transcriber;
        _summarizer = summarizer;
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

            Console.WriteLine("\n----- Summary -----");
            var summary = await _summarizer.SummarizeAsync(transcript, cancellationToken);
            Console.WriteLine(summary);
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