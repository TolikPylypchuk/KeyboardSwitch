namespace KeyboardSwitch.Core.Services.Text;

using TextCopy;

public sealed class ClipboardTextService(
    IClipboard clipboard,
    IUserActivitySimulator simulator,
    IAppSettingsService settingsService,
    IScheduler scheduler,
    ILogger<ClipboardTextService> logger)
    : ITextService
{
    private static readonly TimeSpan MaxTextRestoreDuration = TimeSpan.FromSeconds(3);

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

            if (this.savedClipboardText != null &&
                this.saveTime != null &&
                scheduler.Now - this.saveTime < MaxTextRestoreDuration)
            {
                Thread.Sleep(50);
                await clipboard.SetTextAsync(this.savedClipboardText);
            }

            this.savedClipboardText = null;
            this.saveTime = null;
        }
    }
}
