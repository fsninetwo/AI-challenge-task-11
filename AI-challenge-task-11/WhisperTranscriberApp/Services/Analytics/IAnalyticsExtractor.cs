namespace WhisperTranscriberApp.Services.Analytics;

public interface IAnalyticsExtractor
{
    Task<AnalyticsData> ExtractAnalyticsAsync(string transcript, CancellationToken cancellationToken = default);
} 