using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace WhisperTranscriberApp;

public sealed class OpenAIGptSummarizer : ISummarizer
{
    private readonly HttpClient _httpClient;
    private readonly string _chatModel;

    public OpenAIGptSummarizer(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _chatModel = string.IsNullOrWhiteSpace(options.Value.ChatModel)
            ? "gpt-3.5-turbo"
            : options.Value.ChatModel;
    }

    public async Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new ChatCompletionRequest(
            _chatModel,
            new[]
            {
                new ChatMessage("system", "You are a helpful assistant that summarizes transcripts."),
                new ChatMessage("user", $"Summarize the following transcript in 3-5 sentences:\n\n{text}")
            });

        using var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);
        return completion?.Choices.FirstOrDefault()?.Message.Content.Trim() ?? string.Empty;
    }

    private record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IEnumerable<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")] double Temperature = 0.7,
        [property: JsonPropertyName("max_tokens")] int? MaxTokens = null);

    private record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private class ChatCompletionResponse
    {
        [JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = new();

        public class Choice
        {
            [JsonPropertyName("message")] public ChatMessage Message { get; set; } = default!;
        }
    }
} 