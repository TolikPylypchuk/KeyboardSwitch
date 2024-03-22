namespace KeyboardSwitch.Tests.Services;

public sealed class SwitchServiceTests(ITestOutputHelper output)
{
    private const string English = "en-US";
    private const string EnglishText = "qwerty";

    private const string Ukrainian = "uk-UA";
    private const string UkrainianText = "asdfgh";

    private const string French = "fr-FR";
    private const string FrenchText = "zxcvbn";

    private const string NonConfiguredText = "uiopjklm ";

    private readonly ILogger<SwitchService> logger = XUnitLogger.Create<SwitchService>(output);

    [Property(DisplayName = "SwitchText should set switched text")]
    public async void SwitchText(SwitchDirection direction)
    {
        // Arrange

        var settings = this.CreateSettings(switchLayout: false);
        var layouts = new[] { English, Ukrainian, French }
            .Select(layout => new KeyboardLayout(layout, layout, layout, layout))
            .ToList();

        var textService = Substitute.For<ITextService>();
        textService.GetTextAsync()!.Returns(Task.FromResult(EnglishText));

        var layoutService = Substitute.For<ILayoutService>();
        layoutService.GetKeyboardLayouts().Returns(layouts);
        layoutService.GetCurrentKeyboardLayout().Returns(layouts.Find(layout => layout.Tag.Equals(English)));

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(settings));

        var switchService = new SwitchService(textService, layoutService, settingsService, this.logger);

        // Act

        await switchService.SwitchTextAsync(direction);

        // Assert

        await textService.Received().SetTextAsync(direction == SwitchDirection.Forward ? UkrainianText : FrenchText);
    }

    [Property(DisplayName = "SwitchText should switch layout if it's is enabled")]
    public async void SwitchLayout(SwitchDirection direction, bool switchLayout)
    {
        // Arrange

        var settings = this.CreateSettings(switchLayout: switchLayout);
        var layouts = new[] { English, Ukrainian, French }
            .Select(layout => new KeyboardLayout(layout, layout, layout, layout))
            .ToList();

        var textService = Substitute.For<ITextService>();
        textService.GetTextAsync()!.Returns(Task.FromResult(EnglishText));

        var layoutService = Substitute.For<ILayoutService>();
        layoutService.GetKeyboardLayouts().Returns(layouts);
        layoutService.GetCurrentKeyboardLayout().Returns(layouts.Find(layout => layout.Tag.Equals(English)));

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(settings));

        var switchService = new SwitchService(textService, layoutService, settingsService, this.logger);

        // Act

        await switchService.SwitchTextAsync(direction);

        // Assert

        if (switchLayout)
        {
            layoutService.Received().SwitchCurrentLayout(direction, settings.SwitchSettings);
        } else
        {
            layoutService.DidNotReceive().SwitchCurrentLayout(direction, settings.SwitchSettings);
        }
    }

    [Property(DisplayName = "SwitchText should not switch text which is not configured")]
    public async void SwitchNonConfiguredText(SwitchDirection direction)
    {
        // Arrange

        var settings = this.CreateSettings(switchLayout: false);
        var layouts = new[] { English, Ukrainian, French }
            .Select(layout => new KeyboardLayout(layout, layout, layout, layout))
            .ToList();

        var textService = Substitute.For<ITextService>();
        textService.GetTextAsync()!.Returns(Task.FromResult(NonConfiguredText));

        var layoutService = Substitute.For<ILayoutService>();
        layoutService.GetKeyboardLayouts().Returns(layouts);
        layoutService.GetCurrentKeyboardLayout().Returns(layouts.Find(layout => layout.Tag.Equals(English)));

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(settings));

        var switchService = new SwitchService(textService, layoutService, settingsService, this.logger);

        // Act

        await switchService.SwitchTextAsync(direction);

        // Assert

        await textService.Received().SetTextAsync(NonConfiguredText);
    }

    private AppSettings CreateSettings(bool switchLayout) =>
        new()
        {
            SwitchSettings = new()
            {
                ForwardModifiers = [ModifierMask.Ctrl, ModifierMask.Shift, ModifierMask.None],
                BackwardModifiers = [ModifierMask.Ctrl, ModifierMask.Shift, ModifierMask.Alt],
                PressCount = 2,
                WaitMilliseconds = 400
            },
            CharsByKeyboardLayoutId = ImmutableDictionary.Create<string, string>()
                .Add(English, EnglishText)
                .Add(Ukrainian, UkrainianText)
                .Add(French, FrenchText),
            InstantSwitching = true,
            SwitchLayout = switchLayout,
            ShowUninstalledLayoutsMessage = true,
            AppVersion = new Version(0, 0)
        };
}
