using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using WhisperTranscriberApp.Options;
using Microsoft.AspNetCore.StaticFiles;
using WhisperTranscriberApp.Models;

namespace WhisperTranscriberApp.Services.Transcription;

public class OpenAIWhisperTranscriber : IAudioTranscriber
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private static readonly FileExtensionContentTypeProvider _mimeProvider = new();

    public OpenAIWhisperTranscriber(HttpClient httpClient, IOptions<OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _model = string.IsNullOrWhiteSpace(options.Value.WhisperModel) ? "whisper-1" : options.Value.WhisperModel!;
    }

    public async Task<string> TranscribeAsync(string filePath)
    {
        // Ensure BaseAddress is configured in case DI registration was missed or misconfigured.
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri("https://api.openai.com/");
        }

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
        // Try to get the MIME type from the built-in provider which contains
        // hundreds of mappings (far more than our previous hard-coded list).
        if (_mimeProvider.TryGetContentType(path, out var mimeType) && !string.IsNullOrWhiteSpace(mimeType))
        {
            // The provider returns "video/mp4" for .mp4 which the Whisper endpoint
            // also accepts for audio. However, for consistency we normalise common
            // video/* results to the corresponding audio/* variant because the
            // HTTP spec doesn't forbid this and OpenAI's examples use audio/*.
            if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                mimeType = "audio/" + mimeType[6..];
            }

            return mimeType;
        }

        // Fall back to the original minimal set for a few audio-specific types
        // that the provider might not include yet, then finally octet-stream.
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".flac" => "audio/flac",
            ".m4a" => "audio/mp4",
            ".ogg" or ".oga" => "audio/ogg",
            ".aac" => "audio/aac",
            ".webm" => "audio/webm",
            _ => "application/octet-stream"
        };
    }
} 