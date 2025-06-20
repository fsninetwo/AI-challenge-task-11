using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace WhisperTranscriberApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register OpenAI options from configuration (appsettings.json)
                services.Configure<OpenAIOptions>(context.Configuration.GetSection(OpenAIOptions.SectionName));

                // Configure the typed HTTP client for OpenAI
                services.AddHttpClient<OpenAIWhisperTranscriber>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;

                    var apiKey = string.IsNullOrWhiteSpace(options.ApiKey)
                        ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                        : options.ApiKey;

                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        throw new InvalidOperationException("OpenAI API key not configured (via appsettings 'OpenAI:ApiKey' or 'OPENAI_API_KEY' environment variable).");
                    }

                    var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
                        ? "https://api.openai.com/"
                        : options.BaseUrl;

                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                });

                services.AddTransient<IAudioTranscriber, OpenAIWhisperTranscriber>();
            })
            .Build();

        var transcriber = host.Services.GetRequiredService<IAudioTranscriber>();

        var filePath = args.Length > 0 ? args[0] : "CAR0004.mp3";

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"File '{filePath}' not found.");
            return;
        }

        try
        {
            string transcript = await transcriber.TranscribeAsync(filePath);
            Console.WriteLine("----- Transcript -----");
            Console.WriteLine(transcript);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error during transcription: {ex.Message}");
        }
    }
} 