using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WhisperTranscriberApp.Options;
using WhisperTranscriberApp.Models;

namespace WhisperTranscriberApp.Services.Summarization;

public sealed class OpenAIGptSummarizer : ISummarizer
{
    private readonly HttpClient _httpClient;
    private readonly string _chatModel;
    private readonly double _temperature;

    public OpenAIGptSummarizer(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _chatModel = string.IsNullOrWhiteSpace(options.Value.ChatModel) ? "gpt-3.5-turbo" : options.Value.ChatModel!;
        _temperature = options.Value.Temperature ?? 0.7;
    }

    public async Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new ChatCompletionRequest(
            _chatModel,
            new[]
            {
                new ChatMessage("system", "You are a helpful assistant that summarizes transcripts."),
                new ChatMessage("user", $"Summarize the following transcript in 3-5 sentences:\n\n{text}")
            },
            Temperature: _temperature);

        using var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);
        return completion?.Choices.FirstOrDefault()?.Message.Content.Trim() ?? string.Empty;
    }

    // Nested models moved to ChatCompletionModels.cs
} 