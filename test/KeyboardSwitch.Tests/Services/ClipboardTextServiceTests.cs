using System.Collections.Immutable;

using KeyboardSwitch.Core.Services.Clipboard;
using KeyboardSwitch.Core.Services.Settings;
using KeyboardSwitch.Core.Services.Simulation;
using KeyboardSwitch.Core.Services.Text;
using KeyboardSwitch.Core.Settings;

using SharpHook.Native;

namespace KeyboardSwitch.Tests.Services;

public sealed class ClipboardTextServiceTests(ITestOutputHelper output)
{
    // These will be removed when FsCheck 3.0 is released
    private const string? ExpectedText = "text"; 
    private const string? ExpectedOldText = "old";

    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(16);

    private readonly ILogger<ClipboardTextService> logger = XUnitLogger.Create<ClipboardTextService>(output);

    [Fact(DisplayName = "GetTextAsync should get text from clipboard")]
    public async Task GetTextFromClipboard()
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync().Returns(Task.FromResult(ExpectedText));

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: false)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        var actualText = await textService.GetTextAsync();

        // Assert

        Assert.Equal(ExpectedText, actualText);

        await simulator.DidNotReceive().SimulateCopy();
    }

    [Fact(DisplayName = "GetTextAsync should simulate copying when instant switching is enabled")]
    public async Task SimulateCopy()
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync().Returns(Task.FromResult(ExpectedText));

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: true)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        var actualText = await textService.GetTextAsync();

        // Assert

        Assert.Equal(ExpectedText, actualText);

        await simulator.Received().SimulateCopy();
    }

    [Fact(DisplayName = "SetTextAsync should set text into clipboard")]
    public async Task SetTextIntoClipboard()
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

        await textService.SetTextAsync(ExpectedText ?? String.Empty);

        // Assert

        await clipboard.Received().SetTextAsync(ExpectedText);
        await simulator.DidNotReceive().SimulatePaste();
    }

    [Fact(DisplayName = "SetTextAsync should simulate pasting when instant switching is enabled")]
    public async Task SimulatePaste()
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

        await textService.SetTextAsync(ExpectedText ?? String.Empty);

        // Assert

        await clipboard.Received().SetTextAsync(ExpectedText);
        await simulator.Received().SimulatePaste();
    }

    [Fact(DisplayName =
        "GetTextAsync and SetTextAsync should restore the clipboard state when instant switching is enabled")]
    public async Task RestoreClipboard()
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync().Returns(Task.FromResult(ExpectedOldText), Task.FromResult(ExpectedText));
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: true)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        await textService.GetTextAsync();
        scheduler.AdvanceBy(1);

        var task = textService.SetTextAsync(ExpectedText ?? String.Empty);

        while (!task.IsCompleted)
        {
            scheduler.AdvanceBy(Delay.Ticks);
        }

        // Assert

        await clipboard.Received(2).GetTextAsync();
        await clipboard.Received().SetTextAsync(ExpectedText ?? String.Empty);
        await clipboard.Received().SetTextAsync(ExpectedOldText ?? String.Empty);

        await simulator.Received().SimulateCopy();
        await simulator.Received().SimulatePaste();
    }

    [Fact(DisplayName =
        "GetTextAsync and SetTextAsync should not restore the clipboard state when the delay is too long")]
    public async Task RestoreClipboardTooLong()
    {
        // Arrange

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.GetTextAsync().Returns(Task.FromResult(ExpectedOldText), Task.FromResult(ExpectedText));
        clipboard.SetTextAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var simulator = Substitute.For<IUserActivitySimulator>();

        var settingsService = Substitute.For<IAppSettingsService>();
        settingsService.GetAppSettings().Returns(Task.FromResult(this.CreateSettings(instantSwitching: true)));

        var scheduler = new TestScheduler();

        var textService = new ClipboardTextService(clipboard, simulator, settingsService, scheduler, logger);

        // Act

        await textService.GetTextAsync();
        scheduler.AdvanceBy(ClipboardTextService.MaxTextRestoreDuration.Ticks + 100);

        var task = textService.SetTextAsync(ExpectedText ?? String.Empty);

        while (!task.IsCompleted)
        {
            scheduler.AdvanceBy(Delay.Ticks);
        }

        // Assert

        await clipboard.Received(2).GetTextAsync();
        await clipboard.Received().SetTextAsync(ExpectedText ?? String.Empty);
        await clipboard.DidNotReceive().SetTextAsync(ExpectedOldText ?? String.Empty);

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
