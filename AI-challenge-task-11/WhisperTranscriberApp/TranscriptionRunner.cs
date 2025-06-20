using System.Threading;
using System.Linq;

namespace WhisperTranscriberApp;

/// <summary>
/// Orchestrates the overall flow of the console application: parses arguments, validates the audio file, and outputs the transcript.
/// Placing this logic in its own class keeps <c>Program.cs</c> minimal and improves testability.
/// </summary>
public class TranscriptionRunner
{
    private readonly IAudioTranscriber _transcriber;
    private readonly ISummarizer _summarizer;
    private readonly IAnalyticsExtractor _analytics;

    public TranscriptionRunner(IAudioTranscriber transcriber, ISummarizer summarizer, IAnalyticsExtractor analytics)
    {
        _transcriber = transcriber;
        _summarizer = summarizer;
        _analytics = analytics;
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
            Console.WriteLine("\n================ TRANSCRIPTION =================\n");
            Console.WriteLine(transcript);

            Console.WriteLine("\n================ SUMMARY =======================\n");
            var summary = await _summarizer.SummarizeAsync(transcript, cancellationToken);
            Console.WriteLine(summary);

            Console.WriteLine("\n================ ANALYTICS =====================\n");
            var analytics = await _analytics.ExtractAnalyticsAsync(transcript, cancellationToken);
            Console.WriteLine($"Total words        : {analytics.WordCount}");
            Console.WriteLine($"Speaking speed (WPM): {analytics.SpeakingSpeedWpm:F2}\n");

            if (analytics.FrequentlyMentionedTopics.Any())
            {
                Console.WriteLine("Top frequently mentioned topics (topic : mentions)");
                foreach (var tp in analytics.FrequentlyMentionedTopics.OrderByDescending(t => t.Mentions))
                {
                    Console.WriteLine($" - {tp.Topic,-25} : {tp.Mentions}");
                }
            }
            Console.WriteLine("\n================================================\n");
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