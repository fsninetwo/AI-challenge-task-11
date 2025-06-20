using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using WhisperTranscriberApp.Options;
using WhisperTranscriberApp.Services.Analytics;
using WhisperTranscriberApp.Services.Summarization;
using WhisperTranscriberApp.Services.Transcription;
using Microsoft.Extensions.Configuration;
using WhisperTranscriberApp.Runners;
using WhisperTranscriberApp.Services.Output;

namespace WhisperTranscriberApp.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAIHttpClients(this IServiceCollection services)
    {
        // Register typed clients for each service so that they receive a HttpClient
        // instance whose BaseAddress and Bearer token are properly configured.
        services.AddHttpClient<IAudioTranscriber, OpenAIWhisperTranscriber>(ConfigureClient);
        services.AddHttpClient<ISummarizer, OpenAIGptSummarizer>(ConfigureClient);
        services.AddHttpClient<IAnalyticsExtractor, OpenAIAnalyticsExtractor>(ConfigureClient);
        return services;
    }

    private static void ConfigureClient(IServiceProvider provider, HttpClient client)
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
    }

    public static IServiceCollection AddWhisperTranscriberApp(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));

        // Typed HttpClients and core services
        services.AddOpenAIHttpClients();

        // Result saver does not need an HTTP client
        services.AddTransient<IResultSaver, MarkdownResultSaver>();

        // Runner
        services.AddTransient<TranscriptionRunner>();

        return services;
    }
} 