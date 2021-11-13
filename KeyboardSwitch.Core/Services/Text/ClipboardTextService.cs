namespace KeyboardSwitch.Core.Services.Text;

using TextCopy;

public sealed class ClipboardTextService : ITextService
{
    private static readonly TimeSpan MaxTextRestoreDuration = TimeSpan.FromSeconds(3);

    private readonly IClipboard clipboard;
    private readonly IUserActivitySimulator simulator;
    private readonly IAppSettingsService settingsService;
    private readonly IScheduler scheduler;
    private readonly ILogger<ClipboardTextService> logger;

    private string? savedClipboardText;
    private DateTimeOffset? saveTime;

    public ClipboardTextService(
        IClipboard clipboard,
        IUserActivitySimulator simulator,
        IAppSettingsService settingsService,
        IScheduler scheduler,
        ILogger<ClipboardTextService> logger)
    {
        this.clipboard = clipboard;
        this.simulator = simulator;
        this.settingsService = settingsService;
        this.scheduler = scheduler;
        this.logger = logger;
    }

    public async Task<string?> GetTextAsync()
    {
        var settings = await this.settingsService.GetAppSettingsAsync();

        if (settings.InstantSwitching)
        {
            this.logger.LogDebug("Saving the text from the clipboard and simulating copying");

            this.savedClipboardText = await this.clipboard.GetTextAsync();
            this.saveTime = this.scheduler.Now;
            this.simulator.SimulateCopy();
            Thread.Sleep(50);
        }

        this.logger.LogDebug("Getting the text from the clipboard");

        return await this.clipboard.GetTextAsync();
    }

    public async Task SetTextAsync(string text)
    {
        this.logger.LogDebug("Setting the text into the clipboard");

        await this.clipboard.SetTextAsync(text);

        var settings = await this.settingsService.GetAppSettingsAsync();

        if (settings.InstantSwitching)
        {
            this.logger.LogDebug("Simulating pasting and restoring the text into the clipboard");

            Thread.Sleep(50);
            this.simulator.SimulatePaste();

            if (this.savedClipboardText != null &&
                this.saveTime != null &&
                this.scheduler.Now - this.saveTime < MaxTextRestoreDuration)
            {
                Thread.Sleep(50);
                await this.clipboard.SetTextAsync(this.savedClipboardText);
            }

            this.savedClipboardText = null;
            this.saveTime = null;
        }
    }
}
