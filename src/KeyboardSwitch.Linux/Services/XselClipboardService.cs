using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class XselClipboardService(IScheduler scheduler, ILogger<XselClipboardService> logger)
    : ClipboardServiceBase(scheduler)
{
    private static readonly TimeSpan SmallDelay = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

    public override async Task<string?> GetText()
    {
        try
        {
            this.LogUsingXselToGetText();

            var xsel = this.StartXsel("-o --clipboard");

            if (xsel is null)
            {
                this.LogCouldNotStartXselToCopyText();
                return null;
            }

            var text = await xsel.StandardOutput.ReadToEndAsync();

            await xsel.WaitForExitAsync(this.CancelAfter(OneSecond));
            await Task.Delay(SmallDelay);

            return xsel.ExitCode == 0 && !String.IsNullOrEmpty(text) ? text : null;
        } catch (Exception e)
        {
            this.LogExceptionWhenCopyingText(e);
            return null;
        }
    }

    public override async Task SetText(string text)
    {
        try
        {
            this.LogUsingXselToSetText();

            var xsel = this.StartXsel("-i --clipboard");

            if (xsel is null)
            {
                this.LogCouldNotStartXselToPasteText();
                return;
            }

            await xsel.StandardInput.WriteAsync(text);
            xsel.StandardInput.Close();

            await xsel.WaitForExitAsync(this.CancelAfter(OneSecond));
            await Task.Delay(SmallDelay);
        } catch (Exception e)
        {
            this.LogExceptionWhenPastingText(e);
        }
    }

    private Process? StartXsel(string args) =>
        Process.Start(new ProcessStartInfo()
        {
            FileName = "xsel",
            Arguments = args,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        });

    private CancellationToken CancelAfter(TimeSpan delay)
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(delay);

        return tokenSource.Token;
    }

    [LoggerMessage(LogLevel.Debug, "Using xsel to get text from the clipboard")]
    private partial void LogUsingXselToGetText();

    [LoggerMessage(LogLevel.Error, "Could not start xsel to copy text")]
    private partial void LogCouldNotStartXselToCopyText();

    [LoggerMessage(LogLevel.Error, "Exception when copying text through xsel")]
    private partial void LogExceptionWhenCopyingText(Exception e);

    [LoggerMessage(LogLevel.Debug, "Using xsel to set text into the clipboard")]
    private partial void LogUsingXselToSetText();

    [LoggerMessage(LogLevel.Error, "Could not start xsel to paste text")]
    private partial void LogCouldNotStartXselToPasteText();

    [LoggerMessage(LogLevel.Error, "Exception when pasting text through xsel")]
    private partial void LogExceptionWhenPastingText(Exception e);
}
