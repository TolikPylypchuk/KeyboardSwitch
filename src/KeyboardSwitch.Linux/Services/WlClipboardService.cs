using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class WlClipboardService(IScheduler scheduler, ILogger<WlClipboardService> logger)
    : ExternalClipboardServiceBase(scheduler)
{
    protected override Process? StartGetText() =>
        this.StartWlClipboard("wl-paste", "--no-newline");

    protected override Process? StartSetText() =>
        this.StartWlClipboard("wl-copy", "--trim-newline");

    [LoggerMessage(LogLevel.Debug, "Using wl-paste to get text from the clipboard")]
    protected override partial void LogGetText();

    [LoggerMessage(LogLevel.Error, "Could not start wl-paste to get text")]
    protected override partial void LogCouldNotGetText();

    [LoggerMessage(LogLevel.Error, "Exception when getting text through wl-paste")]
    protected override partial void LogExceptionWhenGettingText(Exception e);

    [LoggerMessage(LogLevel.Debug, "Using wl-copy to set text into the clipboard")]
    protected override partial void LogSetText();

    [LoggerMessage(LogLevel.Error, "Could not start wl-copy to set text")]
    protected override partial void LogCouldNotSetText();

    [LoggerMessage(LogLevel.Error, "Exception when setting text through wl-copy")]
    protected override partial void LogExceptionWhenSettingText(Exception e);

    private Process? StartWlClipboard(string command, string args) =>
        Process.Start(new ProcessStartInfo()
        {
            FileName = command,
            Arguments = args,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        });
}
