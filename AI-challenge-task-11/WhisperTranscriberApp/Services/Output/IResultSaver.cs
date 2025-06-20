using WhisperTranscriberApp.Services.Analytics;

namespace WhisperTranscriberApp.Services.Output;

public interface IResultSaver
{
    Task SaveAsync(string audioFilePath, string transcript, string summary, AnalyticsData analytics, CancellationToken cancellationToken = default);
} 