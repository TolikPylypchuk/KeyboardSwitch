using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class WlClipboardService(IScheduler scheduler, ILogger<WlClipboardService> logger)
    : ExternalClipboardServiceBase(scheduler)
{
    protected override Process? StartCopy() =>
        this.StartWlClipboard("wl-copy");

    protected override Process? StartPaste() =>
        this.StartWlClipboard("wl-paste");

    [LoggerMessage(LogLevel.Debug, "Using wl-copy to get text from the clipboard")]
    protected override partial void LogGetText();

    [LoggerMessage(LogLevel.Error, "Could not start wl-copy to copy text")]
    protected override partial void LogCouldNotCopyText();

    [LoggerMessage(LogLevel.Error, "Exception when copying text through wl-copy")]
    protected override partial void LogExceptionWhenCopyingText(Exception e);

    [LoggerMessage(LogLevel.Debug, "Using wl-paste to set text into the clipboard")]
    protected override partial void LogSetText();

    [LoggerMessage(LogLevel.Error, "Could not start wl-paste to paste text")]
    protected override partial void LogCouldNotPasteText();

    [LoggerMessage(LogLevel.Error, "Exception when pasting text through wl-paste")]
    protected override partial void LogExceptionWhenPastingText(Exception e);

    private Process? StartWlClipboard(string command) =>
        Process.Start(new ProcessStartInfo()
        {
            FileName = command,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        });
}
