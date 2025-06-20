using System.Text.Json.Serialization;

namespace WhisperTranscriberApp.Services.Analytics;

public sealed class AnalyticsData
{
    [JsonPropertyName("word_count")] public int WordCount { get; init; }
    [JsonPropertyName("speaking_speed_wpm")] public double SpeakingSpeedWpm { get; init; }
    [JsonPropertyName("frequently_mentioned_topics")] public List<TopicFrequency> FrequentlyMentionedTopics { get; init; } = new();

    public sealed class TopicFrequency
    {
        [JsonPropertyName("topic")] public string Topic { get; init; } = string.Empty;
        [JsonPropertyName("mentions")] public int Mentions { get; init; }
    }
} 