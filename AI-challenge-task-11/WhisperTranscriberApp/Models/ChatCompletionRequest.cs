using System.Text.Json.Serialization;

namespace WhisperTranscriberApp.Models;

public record ChatCompletionRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IEnumerable<ChatMessage> Messages,
    [property: JsonPropertyName("temperature")] double Temperature = 0.7,
    [property: JsonPropertyName("max_tokens")] int? MaxTokens = null); 