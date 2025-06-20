using WhisperTranscriberApp.Services.Analytics;
using Microsoft.Extensions.Options;
using WhisperTranscriberApp.Options;

namespace WhisperTranscriberApp.Services.Output;

public class MarkdownResultSaver : IResultSaver
{
    private readonly string _outputDir;

    public MarkdownResultSaver(IOptions<OutputOptions> outputOptions)
    {
        var configured = outputOptions.Value.Directory;

        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = "Transcripts";
        }

        _outputDir = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);

        Directory.CreateDirectory(_outputDir);
    }

    public async Task SaveAsync(string audioFilePath, string transcript, string summary, AnalyticsData analytics, CancellationToken cancellationToken = default)
    {
        var outputDir = _outputDir;

        var safeBaseName = Path.GetFileNameWithoutExtension(audioFilePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Create a dedicated subfolder for this analysis (e.g. "Transcripts/<file>_<timestamp>/").
        var analysisDir = Path.Combine(outputDir, $"{safeBaseName}_{timestamp}");
        Directory.CreateDirectory(analysisDir);

        var translationPath = Path.Combine(analysisDir, "transcript.md");
        await File.WriteAllTextAsync(translationPath, $"# Transcript\n\n{transcript}", cancellationToken);

        var summaryPath = Path.Combine(analysisDir, "summary.md");
        await File.WriteAllTextAsync(summaryPath, $"# Summary\n\n{summary}", cancellationToken);

        var analyticsPath = Path.Combine(analysisDir, "analytics.md");
        using (var writer = new StreamWriter(analyticsPath))
        {
            await writer.WriteLineAsync("# Analytics");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"- **Word count:** {analytics.WordCount}");
            await writer.WriteLineAsync($"- **Speaking speed (WPM):** {analytics.SpeakingSpeedWpm:F2}");
            await writer.WriteLineAsync("- **Top topics:**");
            foreach (var tp in analytics.FrequentlyMentionedTopics.OrderByDescending(t => t.Mentions))
            {
                await writer.WriteLineAsync($"  - {tp.Topic}: {tp.Mentions}");
            }
        }

        Console.WriteLine("Files saved to:");
        Console.WriteLine($" {analysisDir}");
    }
} 