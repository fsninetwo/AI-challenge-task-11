using System.Text.Json.Serialization;

namespace WhisperTranscriberApp.Models;

/// <summary>
/// Represents the JSON structure returned by the OpenAI Whisper transcription endpoint.
/// </summary>
public sealed class WhisperResponse
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
} 