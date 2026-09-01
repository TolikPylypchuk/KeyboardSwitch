namespace KeyboardSwitch.Core.Services.Clipboard;

public abstract class ClipboardServiceBase(IScheduler scheduler) : IClipboardService
{
    protected static readonly TimeSpan SmallDelay = TimeSpan.FromMilliseconds(50);

    protected IScheduler Scheduler { get; } = scheduler;

    public abstract Task<string?> GetText();

    public abstract Task SetText(string text);

    public async Task<IAsyncDisposable> SaveClipboardState()
    {
        var savedText = await this.GetText();
        var saveTime = this.Scheduler.Now;

        return new AsyncDisposable(async () =>
        {
            if (!String.IsNullOrEmpty(savedText) && this.Scheduler.Now - saveTime < MaxClipboardRestoreDuration)
            {
                await this.Scheduler.Sleep(SmallDelay);
                await this.SetText(savedText);
            }
        });
    }
}
