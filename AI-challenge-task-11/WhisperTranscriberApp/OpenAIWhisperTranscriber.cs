using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace WhisperTranscriberApp;

public class OpenAIWhisperTranscriber : IAudioTranscriber
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OpenAIWhisperTranscriber(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _model = string.IsNullOrWhiteSpace(options.Value.WhisperModel) ? "whisper-1" : options.Value.WhisperModel;
    }

    public async Task<string> TranscribeAsync(string filePath)
    {
        await using var fileStream = File.OpenRead(filePath);

        using var content = new MultipartFormDataContent();

        // Add the file as stream content
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
        content.Add(fileContent, "file", Path.GetFileName(filePath));

        // Model parameter (can be configured via appsettings - defaults to whisper-1)
        content.Add(new StringContent(_model), "model");

        // Optional: You can specify additional parameters like language, prompt, etc.

        using var response = await _httpClient.PostAsync("v1/audio/transcriptions", content);
        response.EnsureSuccessStatusCode();

        var transcription = await response.Content.ReadFromJsonAsync<WhisperResponse>();
        return transcription?.Text ?? string.Empty;
    }

    private class WhisperResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
} 