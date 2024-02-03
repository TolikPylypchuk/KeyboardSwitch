namespace KeyboardSwitch.Core.Services.Settings;

using System.Reflection;
using System.Text.Json;

using KeyboardSwitch.Core.Exceptions;

internal sealed class JsonSettingsService(
    ILayoutService layoutService,
    IOptions<GlobalSettings> globalSettings,
    ILogger<JsonSettingsService> logger)
    : IAppSettingsService
{
    private readonly FileInfo file = new(Environment.ExpandEnvironmentVariables(globalSettings.Value.SettingsFilePath));
    private readonly Subject<Unit> settingsInvalidated = new();

    private AppSettings? appSettings;

    public IObservable<Unit> SettingsInvalidated =>
        this.settingsInvalidated.AsObservable();

    public async Task<AppSettings> GetAppSettings()
    {
        logger.LogDebug("Getting the app settings");

        if (this.appSettings is not null)
        {
            return this.appSettings;
        }

        if (this.file.Exists)
        {
            using var stream = new BufferedStream(this.file.OpenRead());
            this.appSettings = await JsonSerializer.DeserializeAsync(stream, AppSettingsContext.Default.AppSettings);
        } else
        {
            logger.LogInformation("App settings not found - creating default settings");
            await this.SaveAppSettings(this.CreateDefaultAppSettings());
        }

        if (this.appSettings is null)
        {
            throw new SettingsException("Could not read the app settings");
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
            await this.MigrateSettingsToLatestVersion();
        }

        return this.appSettings;
    }

    private async Task MigrateSettingsToLatestVersion()
    {
        if (this.appSettings is null)
        {
            return;
        }

        var defaultSettings = this.CreateDefaultAppSettings();

        logger.LogInformation(
            "Migrating settings from version {SourceVersion} to {TargetVersion}",
            this.appSettings.AppVersion,
            defaultSettings.AppVersion);

        this.appSettings.AppVersion = defaultSettings.AppVersion;
        this.appSettings.SwitchSettings = defaultSettings.SwitchSettings;

        await this.SaveAppSettings(this.appSettings);
    }

    public async Task SaveAppSettings(AppSettings appSettings)
    {
        logger.LogDebug("Saving the app settings");

        this.file.Directory?.Create();

        using var stream = new BufferedStream(this.file.OpenWrite());
        await JsonSerializer.SerializeAsync(stream, appSettings, AppSettingsContext.Default.AppSettings);

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
            CharsByKeyboardLayoutId = layoutService.GetKeyboardLayouts()
                .ToDictionary(layout => layout.Id, _ => String.Empty),
            InstantSwitching = true,
            SwitchLayout = true,
            ShowUninstalledLayoutsMessage = true,
            AppVersion = this.GetAppVersion()
        };

    private Version GetAppVersion() =>
        Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
}
