namespace KeyboardSwitch.Core.Services.Switching;

public class SwitchService(
    IClipboardService clipboard,
    ILayoutService layoutService,
    IUserActivitySimulator simulator,
    IAppSettingsService settingsService,
    ILogger<SwitchService> logger)
    : ISwitchService
{
    public async Task SwitchText(SwitchDirection direction)
    {
        logger.LogDebug("Switching the text {Direction}", direction.AsString());

        var settings = await settingsService.GetAppSettings();
        IAsyncDisposable? savedClipboardState = null;

        if (settings.InstantSwitching)
        {
            logger.LogDebug("Saving the clipboard state and simulating copying");

            savedClipboardState = await clipboard.SaveClipboardState();
            await simulator.SimulateCopy();
        }

        string? textToSwitch = await clipboard.GetText();

        if (!String.IsNullOrEmpty(textToSwitch))
        {
            var newText = this.MapText(textToSwitch, direction, settings);
            await clipboard.SetText(new String(newText));
        }

        if (settings.InstantSwitching)
        {
            logger.LogDebug("Simulating pasting and restoring the state of the clipboard");

            await simulator.SimulatePaste();

            if (savedClipboardState is not null)
            {
                await savedClipboardState.DisposeAsync();
            }
        }

        if (settings.SwitchLayout)
        {
            layoutService.SwitchCurrentLayout(direction, settings.SwitchSettings);
        }
    }

    private string MapText(string text, SwitchDirection direction, AppSettings settings)
    {
        var allLayouts = layoutService.GetKeyboardLayouts();

        if (direction == SwitchDirection.Backward)
        {
            allLayouts = Enumerable.Reverse(allLayouts).ToList();
        }

        var currentLayout = layoutService.GetCurrentKeyboardLayout();

        var newLayout = allLayouts
            .SkipWhile(layout => layout.Id != currentLayout.Id)
            .Skip(1)
            .FirstOrDefault()
            ?? allLayouts[0];

        string currentChars = settings.CharsByKeyboardLayoutId[currentLayout.Id];
        string newChars = settings.CharsByKeyboardLayoutId[newLayout.Id];

        var mapping = currentChars.Zip(newChars).ToDictionary(chars => chars.First, chars => chars.Second);

        var newText = text
            .Select(ch => mapping.TryGetValue(ch, out char newCh) && newCh != MissingCharacter ? newCh : ch)
            .ToArray();

        return new String(newText);
    }
}
