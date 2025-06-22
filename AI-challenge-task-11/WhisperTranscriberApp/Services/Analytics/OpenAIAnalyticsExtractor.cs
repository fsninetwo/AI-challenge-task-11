using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WhisperTranscriberApp.Options;
using WhisperTranscriberApp.Models;

namespace WhisperTranscriberApp.Services.Analytics;

public sealed class OpenAIAnalyticsExtractor : IAnalyticsExtractor
{
    private readonly HttpClient _httpClient;
    private readonly string _chatModel;
    private readonly double _temperature;

    public OpenAIAnalyticsExtractor(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _chatModel = string.IsNullOrWhiteSpace(options.Value.ChatModel) ? "gpt-3.5-turbo" : options.Value.ChatModel!;
        _temperature = options.Value.Temperature ?? 0.2;
    }

    public async Task<AnalyticsData> ExtractAnalyticsAsync(string transcript, CancellationToken cancellationToken = default)
    {
        var systemPrompt = "You are an AI assistant that analyzes transcripts and returns key statistics in JSON.";
        var userPrompt = "Given the following transcript, calculate:\n" +
                         "1. The total word count as 'word_count'.\n" +
                         "2. Speaking speed in words per minute as 'speaking_speed_wpm' (assume duration is equal to transcript length divided by average speaking speed of 150 WPM if not specified).\n" +
                         "3. A list 'frequently_mentioned_topics' containing at least 3 of the most frequent significant topics (noun phrases) with fields 'topic' and 'mentions'.\n" +
                         "Return strictly a JSON object with those fields.\n\nTranscript:\n" + transcript;

        var request = new ChatCompletionRequest(
            _chatModel,
            new[]
            {
                new ChatMessage("system", systemPrompt),
                new ChatMessage("user", userPrompt)
            },
            Temperature: _temperature);

        using var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(raw);
        var content = completion?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
        var analytics = JsonSerializer.Deserialize<AnalyticsData>(content);
        if (analytics is null)
        {
            throw new InvalidOperationException("Failed to parse analytics JSON from model response.");
        }
        return analytics;
    }

    // Reuse shared chat completion models to avoid duplication.
} 