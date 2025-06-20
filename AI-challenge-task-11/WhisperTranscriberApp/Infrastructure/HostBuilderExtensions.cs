using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WhisperTranscriberApp.Infrastructure;

/// <summary>
/// Extensions for configuring logging on <see cref="IHostBuilder"/> instances.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Adds a simple one-line console logger with <see cref="LogLevel.Warning"/> as the minimum level
    /// and suppresses the noisy <c>System.Net.Http.HttpClient</c> category. This keeps the console
    /// output clean while still surfacing important warnings and errors.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    public static IHostBuilder AddMinimalConsoleLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "[HH:mm:ss] ";
            });

            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        });
    }
} 