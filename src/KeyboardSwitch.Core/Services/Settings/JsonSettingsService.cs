using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;

using KeyboardSwitch.Core.Exceptions;
using KeyboardSwitch.Core.Json;
using KeyboardSwitch.Core.Services.AutoConfiguration;

namespace KeyboardSwitch.Core.Services.Settings;

internal sealed class JsonSettingsService(
    ILayoutService layoutService,
    IAutoConfigurationService autoConfigurationService,
    IFileSystem fileSystem,
    IOptions<GlobalSettings> globalSettings,
    ILogger<JsonSettingsService> logger)
    : IAppSettingsService
{
    private readonly IFileInfo file = fileSystem.FileInfo.New(
        Environment.ExpandEnvironmentVariables(globalSettings.Value.SettingsFilePath));

    private readonly Subject<Unit> settingsInvalidated = new();

    private AppSettings? appSettings;

    public IObservable<Unit> SettingsInvalidated =>
        this.settingsInvalidated.AsObservable();

    public async Task<AppSettings> GetAppSettings(bool strict = false)
    {
        logger.LogDebug("Getting the app settings");

        if (this.appSettings is not null)
        {
            return this.appSettings;
        }

        if (this.file.Exists)
        {
            using var stream = new BufferedStream(this.file.OpenRead());
            this.appSettings = await JsonSerializer.DeserializeAsync(
                stream, KeyboardSwitchJsonContext.Default.AppSettings);
        } else if (strict)
        {
            throw new SettingsNotFoundException("Settings file not found");
        } else
        {
            logger.LogInformation("App settings not found - creating default settings");
            await this.SaveAppSettings(this.CreateDefaultAppSettings());
        }

        if (this.appSettings is null)
        {
            throw new InvalidOperationException("Could not read the app settings");
        }

        var appVersion = this.GetAppVersion();

        if (this.appSettings.AppVersion is null || appSettings.AppVersion > appVersion)
        {
            var version = this.appSettings.AppVersion;
            this.appSettings = null;
            throw new IncompatibleAppVersionException(version);
        }

        if (this.appSettings.AppVersion < appVersion)
        {
            await this.MigrateSettingsToLatestVersion(this.appSettings);
        }

        return this.appSettings;
    }

    private async Task MigrateSettingsToLatestVersion(AppSettings settings)
    {
        var newVersion = this.GetAppVersion();

        logger.LogInformation(
            "Migrating settings from version {SourceVersion} to {TargetVersion}",
            settings.AppVersion,
            newVersion);

        await this.SaveAppSettings(settings with { AppVersion = newVersion });
    }

    public async Task SaveAppSettings(AppSettings appSettings)
    {
        logger.LogDebug("Saving the app settings");

        this.file.Directory?.Create();

        using var stream = new BufferedStream(this.file.OpenWrite());
        await JsonSerializer.SerializeAsync(stream, appSettings, KeyboardSwitchJsonContext.Default.AppSettings);

        this.appSettings = appSettings;
    }

    public void InvalidateAppSettings()
    {
        logger.LogDebug("Invalidating the app settings");

        this.appSettings = null;
        this.settingsInvalidated.OnNext(Unit.Default);
    }

    private AppSettings CreateDefaultAppSettings() =>
        new()
        {
            SwitchSettings = new SwitchSettings
            {
                ForwardModifiers = [ModifierMask.Ctrl, ModifierMask.Shift, ModifierMask.None],
                BackwardModifiers = [ModifierMask.Ctrl, ModifierMask.Shift, ModifierMask.Alt],
                PressCount = 2,
                WaitMilliseconds = 400
            },
            CharsByKeyboardLayoutId = this.GetAutoConfiguredCharMappings(),
            InstantSwitching = true,
            SwitchLayout = true,
            ShowUninstalledLayoutsMessage = true,
            AppVersion = this.GetAppVersion()
        };

    private ImmutableDictionary<string, string> GetAutoConfiguredCharMappings()
    {
        var layouts = this.GetKeyboardLayouts();

        try
        {
            return autoConfigurationService.CreateCharMappings(layouts).ToImmutableDictionary();
        } catch (Exception e)
        {
            logger.LogError(e, "Couldn't get auto-configured character mappings");
            return layouts.ToImmutableDictionary(layout => layout.Id, _ => String.Empty);
        }
    }

    private IReadOnlyList<KeyboardLayout> GetKeyboardLayouts()
    {
        try
        {
            return layoutService.GetKeyboardLayouts();
        } catch (Exception e)
        {
            logger.LogError(e, "Couldn't get keyboard layouts");
            return [];
        }
    }

    private Version GetAppVersion() =>
        Assembly.GetExecutingAssembly()?.GetName().Version ?? new Version(0, 0);
}
