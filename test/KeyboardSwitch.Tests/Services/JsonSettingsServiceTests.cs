using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Options;

using NSubstitute.ExceptionExtensions;

namespace KeyboardSwitch.Tests.Services;

public sealed class JsonSettingsServiceTests(ITestOutputHelper output)
{
    private const string SettingsFile = "settings.json";

    private const string English = "en-US";
    private const string Ukrainian = "uk-UA";
    private const string EnglishChars = "qwerty";
    private const string UkrainianChars = "asdfgh";
    private const int PressCount = 2;
    private const int WaitMilliseconds = 400;

    private static readonly Version Version = Assembly.GetExecutingAssembly()?.GetName()?.Version ?? new Version(0, 0);

    private static readonly string SettingsContent = $$"""
        {
          "SwitchSettings": {
            "ForwardModifiers": [
              "Ctrl",
              "None",
              "None"
            ],
            "BackwardModifiers": [
              "Ctrl",
              "Shift",
              "Alt"
            ],
            "PressCount": {{PressCount}},
            "WaitMilliseconds": {{WaitMilliseconds}}
          },
          "CharsByKeyboardLayoutId": {
            "{{English}}": "{{EnglishChars}}",
            "{{Ukrainian}}": "{{UkrainianChars}}"
          },
          "InstantSwitching": true,
          "SwitchLayout": true,
          "ShowUninstalledLayoutsMessage": true,
          "UseXsel": false,
          "AppVersion": "{{Version}}"
        }
        """;

    private readonly ILogger<JsonSettingsService> logger = XUnitLogger.Create<JsonSettingsService>(output);

    [Fact(DisplayName = "Settings should be read from the settings file")]
    public async Task GetAppSettingsFile()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                [SettingsFile] = new MockFileData(SettingsContent)
            });

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act

        var settings = await settingsService.GetAppSettings();

        // Assert

        Assert.NotNull(settings);
        Assert.NotNull(settings.SwitchSettings);
        Assert.NotNull(settings.CharsByKeyboardLayoutId);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.None, EventMask.None],
            settings.SwitchSettings.ForwardModifiers);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.Alt],
            settings.SwitchSettings.BackwardModifiers);

        Assert.Equal(PressCount, settings.SwitchSettings.PressCount);
        Assert.Equal(WaitMilliseconds, settings.SwitchSettings.WaitMilliseconds);

        Assert.Equal(2, settings.CharsByKeyboardLayoutId.Count);
        Assert.Equal(EnglishChars, settings.CharsByKeyboardLayoutId[English]);
        Assert.Equal(UkrainianChars, settings.CharsByKeyboardLayoutId[Ukrainian]);

        Assert.True(settings.InstantSwitching);
        Assert.True(settings.SwitchLayout);
        Assert.True(settings.ShowUninstalledLayoutsMessage);
        Assert.Equal(Version, settings.AppVersion);
    }

    [Fact(DisplayName = "An exception should be thrown if the settings file is not found in strict mode")]
    public async Task GetAppSettingsStrict()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem();

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act + Assert

        await Assert.ThrowsAsync<SettingsNotFoundException>(() => settingsService.GetAppSettings(strict: true));
    }

    [Fact(DisplayName = "Default app settings should be created in non-strict mode")]
    public async Task GetAppSettingsDefault()
    {
        // Arrange

        var layouts = new List<KeyboardLayout>
            {
                new(English, "English", "US", English),
                new(Ukrainian, "Ukrainian", "Ukrainian (Enhanced)", Ukrainian)
            };

        var layoutService = Substitute.For<ILayoutService>();
        layoutService.GetKeyboardLayouts().Returns(layouts);

        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        autoConfigService.CreateCharMappings(Arg.Is<IEnumerable<KeyboardLayout>>(l => layouts.SequenceEqual(l)))
            .Returns(new Dictionary<string, string>
            {
                [English] = EnglishChars,
                [Ukrainian] = UkrainianChars
            });

        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem();

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act

        var settings = await settingsService.GetAppSettings(strict: false);

        // Assert

        Assert.NotNull(settings);
        Assert.NotNull(settings.SwitchSettings);
        Assert.NotNull(settings.CharsByKeyboardLayoutId);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.None],
            settings.SwitchSettings.ForwardModifiers);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.Alt],
            settings.SwitchSettings.BackwardModifiers);

        Assert.Equal(PressCount, settings.SwitchSettings.PressCount);
        Assert.Equal(WaitMilliseconds, settings.SwitchSettings.WaitMilliseconds);

        Assert.Equal(2, settings.CharsByKeyboardLayoutId.Count);
        Assert.Equal(EnglishChars, settings.CharsByKeyboardLayoutId[English]);
        Assert.Equal(UkrainianChars, settings.CharsByKeyboardLayoutId[Ukrainian]);

        Assert.True(settings.InstantSwitching);
        Assert.True(settings.SwitchLayout);
        Assert.True(settings.ShowUninstalledLayoutsMessage);
        Assert.Equal(Version, settings.AppVersion);

        layoutService.Received().GetKeyboardLayouts();
        autoConfigService.Received().CreateCharMappings(
            Arg.Is<IEnumerable<KeyboardLayout>>(l => layouts.SequenceEqual(l)));
    }

    [Fact(DisplayName = "An exception should be thrown if the settings version is greater than the version of the app")]
    public async Task GetAppSettingsIncompatibleVersion()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();

        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var greaterVersion = new Version(Version.Major + 1, 0);

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            [SettingsFile] = new MockFileData(SettingsContent.Replace(Version.ToString(), greaterVersion.ToString()))
        });

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act + Assert

        var exception = await Assert.ThrowsAsync<IncompatibleAppVersionException>(
            () => settingsService.GetAppSettings());

        Assert.Equal(greaterVersion, exception.Version);
    }

    [Fact(DisplayName = "An exception should be thrown if the settings version is null")]
    public async Task GetAppSettingsNullVersion()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();

        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            [SettingsFile] = new MockFileData(SettingsContent.Replace($"\"{Version}\"", "null"))
        });

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act + Assert

        var exception = await Assert.ThrowsAsync<IncompatibleAppVersionException>(
            () => settingsService.GetAppSettings());

        Assert.Null(exception.Version);
    }

    [Fact(DisplayName = "Settings should be cached after reading")]
    public async Task GetAppSettingsCache()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            [SettingsFile] = new MockFileData(SettingsContent)
        });

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act

        await settingsService.GetAppSettings();
        fileSystem.GetFile(SettingsFile).TextContents = SettingsContent.Replace("true", "false");

        var actualSettings = await settingsService.GetAppSettings();

        // Assert

        Assert.True(actualSettings.InstantSwitching);
        Assert.True(actualSettings.SwitchLayout);
        Assert.True(actualSettings.ShowUninstalledLayoutsMessage);
    }

    [Fact(DisplayName = "Settings should be updated if they have a previous version")]
    public async Task GetAppSettingsMigrateVersion()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();

        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            [SettingsFile] = new MockFileData(SettingsContent.Replace(Version.ToString(), new Version(0, 0).ToString()))
        });

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act

        var settings = await settingsService.GetAppSettings(strict: false);

        // Assert

        Assert.NotNull(settings);
        Assert.NotNull(settings.SwitchSettings);
        Assert.NotNull(settings.CharsByKeyboardLayoutId);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.None, EventMask.None],
            settings.SwitchSettings.ForwardModifiers);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.Alt],
            settings.SwitchSettings.BackwardModifiers);

        Assert.Equal(PressCount, settings.SwitchSettings.PressCount);
        Assert.Equal(WaitMilliseconds, settings.SwitchSettings.WaitMilliseconds);

        Assert.Equal(2, settings.CharsByKeyboardLayoutId.Count);
        Assert.Equal(EnglishChars, settings.CharsByKeyboardLayoutId[English]);
        Assert.Equal(UkrainianChars, settings.CharsByKeyboardLayoutId[Ukrainian]);

        Assert.True(settings.InstantSwitching);
        Assert.True(settings.SwitchLayout);
        Assert.True(settings.ShowUninstalledLayoutsMessage);
        Assert.Equal(Version, settings.AppVersion);
    }

    [Fact(DisplayName = "Default app settings should be created when getting layouts throws an exception")]
    public async Task GetAppSettingsDefaultExceptionLayouts()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        layoutService.GetKeyboardLayouts().Throws<InvalidOperationException>();

        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        autoConfigService.CreateCharMappings(Arg.Is<IEnumerable<KeyboardLayout>>(l => l.IsEmpty()))
            .Returns(new Dictionary<string, string>());

        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem();

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act

        var settings = await settingsService.GetAppSettings(strict: false);

        // Assert

        Assert.NotNull(settings);
        Assert.NotNull(settings.SwitchSettings);
        Assert.NotNull(settings.CharsByKeyboardLayoutId);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.None],
            settings.SwitchSettings.ForwardModifiers);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.Alt],
            settings.SwitchSettings.BackwardModifiers);

        Assert.Equal(PressCount, settings.SwitchSettings.PressCount);
        Assert.Equal(WaitMilliseconds, settings.SwitchSettings.WaitMilliseconds);

        Assert.Equal(0, settings.CharsByKeyboardLayoutId.Count);

        Assert.True(settings.InstantSwitching);
        Assert.True(settings.SwitchLayout);
        Assert.True(settings.ShowUninstalledLayoutsMessage);
        Assert.Equal(Version, settings.AppVersion);

        layoutService.Received().GetKeyboardLayouts();
        autoConfigService.Received().CreateCharMappings(
            Arg.Is<IEnumerable<KeyboardLayout>>(l => l.IsEmpty()));
    }

    [Fact(DisplayName = "Default app settings should be created when getting auto-configuration throws an exception")]
    public async Task GetAppSettingsDefaultExceptionAutoConfig()
    {
        // Arrange

        var layouts = new List<KeyboardLayout>
            {
                new(English, "English", "US", English),
                new(Ukrainian, "Ukrainian", "Ukrainian (Enhanced)", Ukrainian)
            };

        var layoutService = Substitute.For<ILayoutService>();
        layoutService.GetKeyboardLayouts().Returns(layouts);

        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        autoConfigService.CreateCharMappings(Arg.Is<IEnumerable<KeyboardLayout>>(l => layouts.SequenceEqual(l)))
            .Throws<InvalidOperationException>();

        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem();

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act

        var settings = await settingsService.GetAppSettings(strict: false);

        // Assert

        Assert.NotNull(settings);
        Assert.NotNull(settings.SwitchSettings);
        Assert.NotNull(settings.CharsByKeyboardLayoutId);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.None],
            settings.SwitchSettings.ForwardModifiers);

        Assert.Equal(
            [EventMask.Ctrl, EventMask.Shift, EventMask.Alt],
            settings.SwitchSettings.BackwardModifiers);

        Assert.Equal(PressCount, settings.SwitchSettings.PressCount);
        Assert.Equal(WaitMilliseconds, settings.SwitchSettings.WaitMilliseconds);

        Assert.Equal(2, settings.CharsByKeyboardLayoutId.Count);
        Assert.Equal(String.Empty, settings.CharsByKeyboardLayoutId[English]);
        Assert.Equal(String.Empty, settings.CharsByKeyboardLayoutId[Ukrainian]);

        Assert.True(settings.InstantSwitching);
        Assert.True(settings.SwitchLayout);
        Assert.True(settings.ShowUninstalledLayoutsMessage);
        Assert.Equal(Version, settings.AppVersion);

        layoutService.Received().GetKeyboardLayouts();
        autoConfigService.Received().CreateCharMappings(
            Arg.Is<IEnumerable<KeyboardLayout>>(l => layouts.SequenceEqual(l)));
    }

    [Fact(DisplayName = "Invalidating the app settings should remove the cache")]
    public async Task InvalidateAppSettingsCache()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            [SettingsFile] = new MockFileData(SettingsContent)
        });

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        // Act

        await settingsService.GetAppSettings();
        fileSystem.GetFile(SettingsFile).TextContents = SettingsContent.Replace("true", "false");

        settingsService.InvalidateAppSettings();
        var actualSettings = await settingsService.GetAppSettings();

        // Assert

        Assert.False(actualSettings.InstantSwitching);
        Assert.False(actualSettings.SwitchLayout);
        Assert.False(actualSettings.ShowUninstalledLayoutsMessage);
    }

    [Fact(DisplayName = "Invalidating the app settings should emit an observable event")]
    public void InvalidateAppSettingsObservable()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            [SettingsFile] = new MockFileData(SettingsContent)
        });

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        var scheduler = new TestScheduler();
        var observable = scheduler.CreateObserver<Unit>();
        settingsService.SettingsInvalidated.Subscribe(observable);

        // Act

        settingsService.InvalidateAppSettings();
        scheduler.AdvanceBy(1);

        // Assert

        Assert.Equal(1, observable.Messages.Count);
    }

    [Fact(DisplayName = "Saving the app settings should write them")]
    public async Task SaveAppSettings()
    {
        // Arrange

        var layoutService = Substitute.For<ILayoutService>();
        var autoConfigService = Substitute.For<IAutoConfigurationService>();
        var globalSettings = new GlobalSettings { SettingsFilePath = SettingsFile };

        var fileSystem = new MockFileSystem();

        var settingsService = new JsonSettingsService(
            layoutService, autoConfigService, fileSystem, Options.Create(globalSettings), this.logger);

        var settings = new AppSettings
        {
            SwitchSettings = new()
            {
                ForwardModifiers = [EventMask.Ctrl, EventMask.None, EventMask.None],
                BackwardModifiers = [EventMask.Ctrl, EventMask.Shift, EventMask.Alt],
                PressCount = PressCount,
                WaitMilliseconds = WaitMilliseconds
            },
            CharsByKeyboardLayoutId = ImmutableDictionary.Create<string, string>()
                .Add(English, EnglishChars)
                .Add(Ukrainian, UkrainianChars),
            InstantSwitching = true,
            SwitchLayout = true,
            ShowUninstalledLayoutsMessage = true,
            UseXsel = false,
            AppVersion = Version
        };

        // Act

        await settingsService.SaveAppSettings(settings);

        // Assert

        var file = fileSystem.GetFile(SettingsFile);
        Assert.NotNull(file);

        var expectedJson = JsonNode.Parse(SettingsContent);
        var actualJson = JsonNode.Parse(file.TextContents);
        Assert.True(JsonNode.DeepEquals(expectedJson, actualJson));
    }
}
