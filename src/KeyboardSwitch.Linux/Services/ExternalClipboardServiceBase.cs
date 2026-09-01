using System.Reactive.Concurrency;

namespace KeyboardSwitch.Linux.Services;

internal abstract class ExternalClipboardServiceBase(IScheduler scheduler) : ClipboardServiceBase(scheduler)
{
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

    public override async Task<string?> GetText()
    {
        try
        {
            this.LogGetText();
            var copy = this.StartGetText();

            if (copy is null)
            {
                this.LogCouldNotGetText();
                return null;
            }

            var text = await copy.StandardOutput.ReadToEndAsync();

            await copy.WaitForExitAsync(this.CancelAfter(OneSecond));
            await this.Scheduler.Sleep(SmallDelay);

            return copy.ExitCode == 0 && !String.IsNullOrEmpty(text) ? text : null;
        } catch (Exception e)
        {
            this.LogExceptionWhenGettingText(e);
            return null;
        }
    }

    public override async Task SetText(string text)
    {
        try
        {
            this.LogSetText();
            var paste = this.StartSetText();

            if (paste is null)
            {
                this.LogCouldNotSetText();
                return;
            }

            await paste.StandardInput.WriteAsync(text);
            paste.StandardInput.Close();

            await paste.WaitForExitAsync(this.CancelAfter(OneSecond));
            await this.Scheduler.Sleep(SmallDelay);
        } catch (Exception e)
        {
            this.LogExceptionWhenSettingText(e);
        }
    }

    protected abstract Process? StartGetText();

    protected abstract Process? StartSetText();

    protected abstract void LogGetText();

    protected abstract void LogCouldNotGetText();

    protected abstract void LogExceptionWhenGettingText(Exception e);

    protected abstract void LogSetText();

    protected abstract void LogCouldNotSetText();

    protected abstract void LogExceptionWhenSettingText(Exception e);

    private CancellationToken CancelAfter(TimeSpan delay)
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(delay);

        return tokenSource.Token;
    }
}
