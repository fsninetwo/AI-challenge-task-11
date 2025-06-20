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
        services.AddHttpClient<OpenAIWhisperTranscriber>(ConfigureClient);
        services.AddHttpClient<OpenAIGptSummarizer>(ConfigureClient);
        services.AddHttpClient<OpenAIAnalyticsExtractor>(ConfigureClient);
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

        // Typed HttpClients
        services.AddOpenAIHttpClients();

        // Core services
        services.AddTransient<IAudioTranscriber, OpenAIWhisperTranscriber>();
        services.AddTransient<ISummarizer, OpenAIGptSummarizer>();
        services.AddTransient<IAnalyticsExtractor, OpenAIAnalyticsExtractor>();
        services.AddTransient<IResultSaver, MarkdownResultSaver>();

        // Runner
        services.AddTransient<TranscriptionRunner>();

        return services;
    }
} 