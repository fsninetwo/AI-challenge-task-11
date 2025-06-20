using System.Linq;
using System.Text;
using WhisperTranscriberApp.Services.Analytics;
using WhisperTranscriberApp.Services.Summarization;
using WhisperTranscriberApp.Services.Transcription;
using WhisperTranscriberApp.Services.Output;

namespace WhisperTranscriberApp.Runners;

/// <summary>
/// Orchestrates the overall CLI workflow: transcription, summarization, analytics.
/// </summary>
public class TranscriptionRunner
{
    private readonly IAudioTranscriber _transcriber;
    private readonly ISummarizer _summarizer;
    private readonly IAnalyticsExtractor _analytics;
    private readonly IResultSaver _resultSaver;

    public TranscriptionRunner(IAudioTranscriber transcriber, ISummarizer summarizer, IAnalyticsExtractor analytics, IResultSaver resultSaver)
    {
        _transcriber = transcriber;
        _summarizer = summarizer;
        _analytics = analytics;
        _resultSaver = resultSaver;
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        Console.OutputEncoding = Encoding.UTF8;
        ShowBanner();

        // If file path argument provided, process single file then exit.
        if (args.Length > 0)
        {
            await ProcessFileAsync(args[0], cancellationToken);
            return;
        }

        // Interactive loop
        while (true)
        {
            Console.Write("Enter path to audio file (or 'exit' to quit): ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                break;
            if (string.Equals(input, "help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp();
                continue;
            }
            if (string.IsNullOrWhiteSpace(input))
                continue;

            await ProcessFileAsync(input, cancellationToken);
        }
    }

    private async Task ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        filePath = filePath.Trim('"');

        if (!File.Exists(filePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ File '{filePath}' not found.\nType 'help' for usage instructions.\n");
            Console.ResetColor();
            return;
        }

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Transcribing... please wait\n");
            Console.ResetColor();

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

            await _resultSaver.SaveAsync(filePath, transcript, summary, analytics, cancellationToken);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Processing complete!\n");
            Console.ResetColor();
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operation was cancelled.\n");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}\nType 'help' for usage instructions.\n");
            Console.ResetColor();
        }
    }

    private static void ShowBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================");
        Console.WriteLine("   WhisperTranscriberApp - Console Edition   ");
        Console.WriteLine("============================================\n");
        Console.ResetColor();
        ShowHelp();
    }

    private static void ShowHelp()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Commands:");
        Console.ResetColor();
        Console.WriteLine("  <path>   - transcribe the specified audio file (any common audio format)");
        Console.WriteLine("  help     - show this help");
        Console.WriteLine("  exit     - quit the application\n");
    }
} 