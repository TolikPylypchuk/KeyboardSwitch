namespace KeyboardSwitch.Core.Services.Switching;

public class SwitchService(
    ITextService textService,
    ILayoutService layoutService,
    IAppSettingsService settingsService,
    ILogger<SwitchService> logger)
    : ISwitchService
{
    public async Task SwitchTextAsync(SwitchDirection direction)
    {
        logger.LogDebug("Switching the text {Direction}", direction.AsString());

        string? textToSwitch = await textService.GetTextAsync();

        var settings = await settingsService.GetAppSettings();

        if (!String.IsNullOrEmpty(textToSwitch))
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

            await textService.SetTextAsync(new String(textToSwitch
                .Select(ch => mapping.TryGetValue(ch, out char newCh) && newCh != MissingCharacter ? newCh : ch)
                .ToArray()));
        }

        if (settings.SwitchLayout)
        {
            layoutService.SwitchCurrentLayout(direction, settings.SwitchSettings);
        }
    }
}
