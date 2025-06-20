namespace WhisperTranscriberApp;

public interface IAnalyticsExtractor
{
    Task<AnalyticsData> ExtractAnalyticsAsync(string transcript, CancellationToken cancellationToken = default);
} 