using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Core;
using Serilog.Events;

using SharpHook.Logging;

using ILogger = Serilog.ILogger;
using UioHookLogLevel = SharpHook.Data.LogLevel;

namespace KeyboardSwitch.Core.Logging;

[ExcludeFromCodeCoverage]
public static class SerilogLoggerFactory
{
    private const string LibUioHookContextName = "libuiohook";

    public static Logger CreateLogger(IConfiguration configuration, bool addLibUioHookLogging)
    {
        var settings = new LoggingSettings();
        configuration.GetRequiredSection("Logger").Bind(settings);

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(settings.MinimumLevel)
            .MinimumLevel.Override("Avalonia", LogEventLevel.Warning)
            .MinimumLevel.Override("ReactiveUI", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
            .MinimumLevel.Override(LibUioHookContextName, LogEventLevel.Warning)
            .WriteTo.Debug(outputTemplate: settings.OutputTemplate)
            .WriteTo.Console(outputTemplate: settings.OutputTemplate)
            .WriteTo.Logger(config => config
                .WriteTo.Async(writeTo => writeTo.File(
                    Environment.ExpandEnvironmentVariables(settings.LogFilePath),
                    outputTemplate: settings.OutputTemplate,
                    fileSizeLimitBytes: settings.MaxFileSize,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: settings.MaxRetainedFiles,
                    shared: true)))
            .CreateLogger();

        if (addLibUioHookLogging)
        {
            var libuiohookLogger = logger.ForContext(
                Serilog.Core.Constants.SourceContextPropertyName, LibUioHookContextName);

            var logSource = LogSource.RegisterOrGet(minLevel: UioHookLogLevel.Warn);
            logSource.MessageLogged += (sender, e) => LogLibUioHookEntry(libuiohookLogger, e.LogEntry);
        }

        return logger;
    }

    private static void LogLibUioHookEntry(ILogger logger, LogEntry logEntry) =>
        logger.Write(logEntry.Level.MapToSerilogLogLevel(), logEntry.FullText);
}

file static class Extensions
{
    extension(UioHookLogLevel level)
    {
        public LogEventLevel MapToSerilogLogLevel() =>
            level switch
            {
                UioHookLogLevel.Error => LogEventLevel.Error,
                UioHookLogLevel.Warn => LogEventLevel.Warning,
                UioHookLogLevel.Info => LogEventLevel.Information,
                UioHookLogLevel.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Verbose
            };
    }
}
