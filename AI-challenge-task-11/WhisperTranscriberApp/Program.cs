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

                    if (string.IsNullOrWhiteSpace(options.BaseUrl))
                    {
                        throw new InvalidOperationException("OpenAI BaseUrl not configured (via appsettings 'OpenAI:BaseUrl').");
                    }

                    var baseUrl = options.BaseUrl!.EndsWith("/") ? options.BaseUrl : options.BaseUrl + "/";

                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                });

                services.AddHttpClient<OpenAIGptSummarizer>((provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                    var apiKey = string.IsNullOrWhiteSpace(options.ApiKey)
                        ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                        : options.ApiKey;
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        throw new InvalidOperationException("OpenAI API key not configured.");
                    }
                    if (string.IsNullOrWhiteSpace(options.BaseUrl))
                    {
                        throw new InvalidOperationException("OpenAI BaseUrl not configured.");
                    }
                    var baseUrl = options.BaseUrl.EndsWith("/") ? options.BaseUrl : options.BaseUrl + "/";
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                });

                services.AddTransient<IAudioTranscriber, OpenAIWhisperTranscriber>();
                services.AddTransient<ISummarizer, OpenAIGptSummarizer>();
                services.AddTransient<TranscriptionRunner>();
            })
            .Build();

        var runner = host.Services.GetRequiredService<TranscriptionRunner>();
        await runner.ExecuteAsync(args);
    }
} 