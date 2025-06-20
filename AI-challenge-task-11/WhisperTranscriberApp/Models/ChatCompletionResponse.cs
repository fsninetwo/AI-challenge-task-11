using System.Text.Json.Serialization;

namespace WhisperTranscriberApp.Models;

public class ChatCompletionResponse
{
    [JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = new();

    public class Choice
    {
        [JsonPropertyName("message")] public ChatMessage Message { get; set; } = default!;
    }
} 