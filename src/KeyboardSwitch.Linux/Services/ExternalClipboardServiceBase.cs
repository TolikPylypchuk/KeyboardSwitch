using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal abstract class ExternalClipboardServiceBase(IScheduler scheduler) : ClipboardServiceBase(scheduler)
{
    private static readonly TimeSpan SmallDelay = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

    public override async Task<string?> GetText()
    {
        try
        {
            this.LogGetText();
            var copy = this.StartCopy();

            if (copy is null)
            {
                this.LogCouldNotCopyText();
                return null;
            }

            var text = await copy.StandardOutput.ReadToEndAsync();

            await copy.WaitForExitAsync(this.CancelAfter(OneSecond));
            await Task.Delay(SmallDelay);

            return copy.ExitCode == 0 && !String.IsNullOrEmpty(text) ? text : null;
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
            this.LogSetText();
            var paste = this.StartPaste();

            if (paste is null)
            {
                this.LogCouldNotPasteText();
                return;
            }

            await paste.StandardInput.WriteAsync(text);
            paste.StandardInput.Close();

            await paste.WaitForExitAsync(this.CancelAfter(OneSecond));
            await Task.Delay(SmallDelay);
        } catch (Exception e)
        {
            this.LogExceptionWhenPastingText(e);
        }
    }

    protected abstract Process? StartCopy();

    protected abstract Process? StartPaste();

    protected abstract void LogGetText();

    protected abstract void LogCouldNotCopyText();

    protected abstract void LogExceptionWhenCopyingText(Exception e);

    protected abstract void LogSetText();

    protected abstract void LogCouldNotPasteText();

    protected abstract void LogExceptionWhenPastingText(Exception e);

    private CancellationToken CancelAfter(TimeSpan delay)
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(delay);

        return tokenSource.Token;
    }
}
