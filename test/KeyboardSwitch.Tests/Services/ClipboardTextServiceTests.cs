namespace KeyboardSwitch.Tests.Services;

public sealed class ClipboardTextServiceTests(ITestOutputHelper output)
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(16);

    private readonly ILogger<ClipboardTextService> logger = XUnitLogger.Create<ClipboardTextService>(output);

    [Property(DisplayName = "GetTextAsync should get text from clipboard")]
    public async void GetTextFromClipboard(string? expectedText)
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync().Returns(Task.FromResult(expectedText));

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: false)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        var actualText = await textService.GetTextAsync();

        // Assert

        Assert.Equal(expectedText, actualText);

        await simulator.DidNotReceive().SimulateCopy();
    }

    [Property(DisplayName = "GetTextAsync should simulate copying when instant switching is enabled")]
    public async void SimulateCopy(string? expectedText)
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync().Returns(Task.FromResult(expectedText));

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: true)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        var actualText = await textService.GetTextAsync();

        // Assert

        Assert.Equal(expectedText, actualText);

        await simulator.Received().SimulateCopy();
    }

    [Property(DisplayName = "SetTextAsync should set text into clipboard")]
    public async void SetTextIntoClipboard(NonEmptyString expectedText)
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: false)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        await textService.SetTextAsync(expectedText.Get);

        // Assert

        await clipboard.Received().SetTextAsync(expectedText.Get);
        await simulator.DidNotReceive().SimulatePaste();
    }

    [Property(DisplayName = "SetTextAsync should simulate pasting when instant switching is enabled")]
    public async void SimulatePaste(NonEmptyString expectedText)
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: true)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        await textService.SetTextAsync(expectedText.Get);

        // Assert

        await clipboard.Received().SetTextAsync(expectedText.Get);
        await simulator.Received().SimulatePaste();
    }

    [Property(DisplayName =
        "GetTextAsync and SetTextAsync should restore the clipboard state when instant switching is enabled")]
    public async void RestoreClipboard(NonEmptyString expectedText, NonEmptyString expectedOldText)
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync()!.Returns(Task.FromResult(expectedOldText.Get), Task.FromResult(expectedText.Get));
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: true)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        await textService.GetTextAsync();
        scheduler.AdvanceBy(1);

        var task = textService.SetTextAsync(expectedText.Get);

        while (!task.IsCompleted)
        {
            scheduler.AdvanceBy(Delay.Ticks);
        }

        // Assert

        await clipboard.Received(2).GetTextAsync();
        await clipboard.Received().SetTextAsync(expectedText.Get);
        await clipboard.Received().SetTextAsync(expectedOldText.Get);

        await simulator.Received().SimulateCopy();
        await simulator.Received().SimulatePaste();
    }

    [Property(DisplayName =
        "GetTextAsync and SetTextAsync should not restore the clipboard state when the delay is too long")]
    public async void RestoreClipboardTooLong(NonEmptyString expectedText, NonEmptyString expectedOldText)
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync()!.Returns(Task.FromResult(expectedOldText.Get), Task.FromResult(expectedText.Get));
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: true)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        await textService.GetTextAsync();
        scheduler.AdvanceBy(ClipboardTextService.MaxTextRestoreDuration.Ticks + 100);

        var task = textService.SetTextAsync(expectedText.Get);

        while (!task.IsCompleted)
        {
            scheduler.AdvanceBy(Delay.Ticks);
        }

        // Assert

        await clipboard.Received(2).GetTextAsync();
        await clipboard.Received().SetTextAsync(expectedText.Get);
        await clipboard.DidNotReceive().SetTextAsync(expectedOldText.Get);

        await simulator.Received().SimulateCopy();
        await simulator.Received().SimulatePaste();
    }

    private AppSettings CreateSettings(bool instantSwitching) =>
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
                .Add("en-US", "qwerty")
                .Add("uk-UA", "asdfgh"),
            InstantSwitching = instantSwitching,
            SwitchLayout = true,
            ShowUninstalledLayoutsMessage = true,
            AppVersion = new Version(0, 0)
        };
}
