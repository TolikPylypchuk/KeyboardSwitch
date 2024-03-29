namespace KeyboardSwitch.Core.Services.Text;

public sealed class ClipboardTextService(
    IClipboardService clipboard,
    IUserActivitySimulator simulator,
    IAppSettingsService settingsService,
    IScheduler scheduler,
    ILogger<ClipboardTextService> logger)
    : ITextService
{
    internal static readonly TimeSpan MaxTextRestoreDuration = TimeSpan.FromSeconds(3);

    private string? savedClipboardText;
    private DateTimeOffset? saveTime;

    public async Task<string?> GetTextAsync()
    {
        var settings = await settingsService.GetAppSettings();

        if (settings.InstantSwitching)
        {
            logger.LogDebug("Saving the text from the clipboard and simulating copying");

            this.savedClipboardText = await clipboard.GetTextAsync();
            this.saveTime = scheduler.Now;

            await simulator.SimulateCopy();
        }

        logger.LogDebug("Getting the text from the clipboard");

        return await clipboard.GetTextAsync();
    }

    public async Task SetTextAsync(string text)
    {
        logger.LogDebug("Setting the text into the clipboard");

        await clipboard.SetTextAsync(text);

        var settings = await settingsService.GetAppSettings();

        if (settings.InstantSwitching)
        {
            logger.LogDebug("Simulating pasting and restoring the text into the clipboard");

            await simulator.SimulatePaste();

            if (this.savedClipboardText is not null &&
                this.saveTime is not null &&
                scheduler.Now - this.saveTime < MaxTextRestoreDuration)
            {
                await scheduler.Sleep(TimeSpan.FromMilliseconds(50));
                await clipboard.SetTextAsync(this.savedClipboardText);
            }

            this.savedClipboardText = null;
            this.saveTime = null;
        }
    }
}
