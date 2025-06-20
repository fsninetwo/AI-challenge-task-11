using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WhisperTranscriberApp.Options;

namespace WhisperTranscriberApp.Services.Transcription;

public class OpenAIWhisperTranscriber : IAudioTranscriber
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OpenAIWhisperTranscriber(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _model = string.IsNullOrWhiteSpace(options.Value.WhisperModel) ? "whisper-1" : options.Value.WhisperModel!;
    }

    public async Task<string> TranscribeAsync(string filePath)
    {
        await using var fileStream = File.OpenRead(filePath);

        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        // Determine MIME type based on file extension so that a wide variety of
        // audio formats (mp3, wav, flac, ogg, m4a, etc.) are accepted by the
        // OpenAI Whisper endpoint. Falls back to application/octet-stream if
        // the extension is unknown.
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(filePath));
        content.Add(fileContent, "file", Path.GetFileName(filePath));

        content.Add(new StringContent(_model), "model");

        using var response = await _httpClient.PostAsync("v1/audio/transcriptions", content);
        response.EnsureSuccessStatusCode();

        var transcription = await response.Content.ReadFromJsonAsync<WhisperResponse>();
        return transcription?.Text ?? string.Empty;
    }

    // Returns the IANA media type for the given audio file path.
    private static string GetMimeType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".mp3" or ".mpeg" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".ogg" or ".oga" => "audio/ogg",
            ".m4a" or ".mp4" => "audio/mp4",
            ".aac" => "audio/aac",
            ".webm" => "audio/webm",
            _ => "application/octet-stream"
        };
    }

    private class WhisperResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
} 