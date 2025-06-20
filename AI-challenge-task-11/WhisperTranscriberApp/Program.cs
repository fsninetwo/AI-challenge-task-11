using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WhisperTranscriberApp.Options;
using WhisperTranscriberApp.Runners;
using WhisperTranscriberApp.Infrastructure;

namespace WhisperTranscriberApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register all application services and HTTP clients via extension
                services.AddWhisperTranscriberApp(context.Configuration);
            })
            .Build();

        var runner = host.Services.GetRequiredService<TranscriptionRunner>();
        await runner.ExecuteAsync(args);
    }
} 