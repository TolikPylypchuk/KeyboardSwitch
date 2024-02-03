namespace KeyboardSwitch.Core.Logging;

using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Core;
using Serilog.Filters;

public static class SerilogLoggerFactory
{
    public static Logger CreateLogger(IConfiguration configuration)
    {
        var settings = new LoggingSettings();
        configuration.GetRequiredSection("Logger").Bind(settings);

        return new LoggerConfiguration()
                .MinimumLevel.Is(settings.MinimumLevel)
                .WriteTo.Debug(outputTemplate: settings.OutputTemplate)
                .WriteTo.Console(outputTemplate: settings.OutputTemplate)
                .WriteTo.Logger(config => config
                    .WriteTo.Async(writeTo => writeTo.File(
                        Environment.ExpandEnvironmentVariables(settings.LogFilePath),
                        outputTemplate: settings.OutputTemplate,
                        fileSizeLimitBytes: settings.MaxFileSize,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: settings.MaxRetainedFiles,
                        shared: true))
                    .Filter.ByIncludingOnly(Matching.FromSource(nameof(KeyboardSwitch))))
                .Enrich.FromLogContext()
                .CreateLogger();
    }
}
