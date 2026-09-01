using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class XselClipboardService(IScheduler scheduler, ILogger<XselClipboardService> logger)
    : ExternalClipboardServiceBase(scheduler)
{
    protected override Process? StartGetText() =>
        this.StartXsel("-o --clipboard");

    protected override Process? StartSetText() =>
        this.StartXsel("-i --clipboard");

    [LoggerMessage(LogLevel.Debug, "Using xsel to get text from the clipboard")]
    protected override partial void LogGetText();

    [LoggerMessage(LogLevel.Error, "Could not start xsel to get text")]
    protected override partial void LogCouldNotGetText();

    [LoggerMessage(LogLevel.Error, "Exception when getting text through xsel")]
    protected override partial void LogExceptionWhenGettingText(Exception e);

    [LoggerMessage(LogLevel.Debug, "Using xsel to set text into the clipboard")]
    protected override partial void LogSetText();

    [LoggerMessage(LogLevel.Error, "Could not start xsel to set text")]
    protected override partial void LogCouldNotSetText();

    [LoggerMessage(LogLevel.Error, "Exception when setting text through xsel")]
    protected override partial void LogExceptionWhenSettingText(Exception e);

    private Process? StartXsel(string args) =>
        Process.Start(new ProcessStartInfo()
        {
            FileName = "xsel",
            Arguments = args,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        });
}
