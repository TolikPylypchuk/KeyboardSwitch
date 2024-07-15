namespace KeyboardSwitch.Core.Services.Clipboard;

public abstract class ClipboardServiceBase(IScheduler scheduler) : IClipboardService
{
    public abstract Task<string?> GetText();

    public abstract Task SetText(string text);

    public async Task<IAsyncDisposable> SaveClipboardState()
    {
        var savedText = await this.GetText();
        var saveTime = scheduler.Now;

        return new AsyncDisposable(async () =>
        {
            if (!String.IsNullOrEmpty(savedText) && scheduler.Now - saveTime < MaxClipboardRestoreDuration)
            {
                await scheduler.Sleep(TimeSpan.FromMilliseconds(50));
                await this.SetText(savedText);
            }
        });
    }
}
