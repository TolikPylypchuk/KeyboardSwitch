namespace KeyboardSwitch.Core.Settings;

using Serilog.Events;

public sealed class LoggingSettings
{
    public string LogFilePath { get; set; } = String.Empty;
    public LogEventLevel MinimumLevel { get; set; }
    public int MaxFileSize { get; set; }
    public int MaxRetainedFiles { get; set; }
    public string OutputTemplate { get; set; } = String.Empty;
}
