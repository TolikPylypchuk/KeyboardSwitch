using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal sealed class XselClipboardService(IScheduler scheduler, ILogger<XselClipboardService> logger)
    : ClipboardServiceBase(scheduler)
{
    private static readonly TimeSpan SmallDelay = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

    public override async Task<string?> GetText()
    {
        try
        {
            logger.LogDebug("Using xsel to get text from the clipboard");

            var xsel = this.StartXsel("-o --clipboard");

            if (xsel is null)
            {
                logger.LogError("Could not start xsel to copy text");
                return null;
            }

            var text = await xsel.StandardOutput.ReadToEndAsync();

            await xsel.WaitForExitAsync(this.CancelAfter(OneSecond));
            await Task.Delay(SmallDelay);

            return xsel.ExitCode == 0 && !String.IsNullOrEmpty(text) ? text : null;
        } catch (Exception e)
        {
            logger.LogError(e, "Exception when copying text through xsel");
            return null;
        }
    }

    public override async Task SetText(string text)
    {
        try
        {
            logger.LogDebug("Using xsel to set text into the clipboard");

            var xsel = this.StartXsel("-i --clipboard");

            if (xsel is null)
            {
                logger.LogError("Could not start xsel to paste text");
                return;
            }

            await xsel.StandardInput.WriteAsync(text);
            xsel.StandardInput.Close();

            await xsel.WaitForExitAsync(this.CancelAfter(OneSecond));
            await Task.Delay(SmallDelay);
        } catch (Exception e)
        {
            logger.LogError(e, "Exception when pasting text through xsel");
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
}
