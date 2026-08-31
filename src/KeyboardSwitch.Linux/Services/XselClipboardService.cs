using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class XselClipboardService(IScheduler scheduler, ILogger<XselClipboardService> logger)
    : ExternalClipboardServiceBase(scheduler)
{
    protected override Process? StartCopy() =>
        this.StartXsel("-o --clipboard");

    protected override Process? StartPaste() =>
        this.StartXsel("-i --clipboard");

    [LoggerMessage(LogLevel.Debug, "Using xsel to get text from the clipboard")]
    protected override partial void LogGetText();

    [LoggerMessage(LogLevel.Error, "Could not start xsel to copy text")]
    protected override partial void LogCouldNotCopyText();

    [LoggerMessage(LogLevel.Error, "Exception when copying text through xsel")]
    protected override partial void LogExceptionWhenCopyingText(Exception e);

    [LoggerMessage(LogLevel.Debug, "Using xsel to set text into the clipboard")]
    protected override partial void LogSetText();

    [LoggerMessage(LogLevel.Error, "Could not start xsel to paste text")]
    protected override partial void LogCouldNotPasteText();

    [LoggerMessage(LogLevel.Error, "Exception when pasting text through xsel")]
    protected override partial void LogExceptionWhenPastingText(Exception e);

    private Process? StartXsel(string args) =>
        Process.Start(new ProcessStartInfo()
        {
            FileName = "xsel",
            Arguments = args,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        });
}
