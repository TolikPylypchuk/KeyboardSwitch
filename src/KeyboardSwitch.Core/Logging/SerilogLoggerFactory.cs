using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace KeyboardSwitch.Core.Logging;

[ExcludeFromCodeCoverage]
public static class SerilogLoggerFactory
{
    public static Logger CreateLogger(IConfiguration configuration)
    {
        var settings = new LoggingSettings();
        configuration.GetRequiredSection("Logger").Bind(settings);

        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(settings.MinimumLevel)
            .MinimumLevel.Override("Avalonia", LogEventLevel.Warning)
            .MinimumLevel.Override("ReactiveUI", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
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
    }
}
