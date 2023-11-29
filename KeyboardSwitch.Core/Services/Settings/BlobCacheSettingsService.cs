namespace KeyboardSwitch.Core.Services.Settings;

using System.Reflection;

using Akavache;

internal sealed class BlobCacheSettingsService : AsyncDisposable, IAppSettingsService
{
    private readonly IBlobCache cache;
    private readonly ILayoutService layoutService;
    private readonly ILogger<BlobCacheSettingsService> logger;
    private readonly Subject<Unit> settingsInvalidated = new();

    private AppSettings? appSettings;

    public BlobCacheSettingsService(
        IBlobCache cache,
        ILayoutService layoutService,
        ILogger<BlobCacheSettingsService> logger)
    {
        this.cache = cache;
        this.layoutService = layoutService;
        this.logger = logger;

        this.settingsInvalidated.Subscribe(this.layoutService.SettingsInvalidated);
    }

    public IObservable<Unit> SettingsInvalidated =>
        this.settingsInvalidated.AsObservable();

    public async Task<AppSettings> GetAppSettings()
    {
        this.ThrowIfDisposed();

        if (this.appSettings == null)
        {
            this.logger.LogDebug("Getting the app settings");

            if (await this.cache.ContainsKey(AppSettings.CacheKey))
            {
                this.appSettings = await this.cache.GetObject<AppSettings>(AppSettings.CacheKey);
            } else
            {
                this.logger.LogInformation("App settings not found - creating default settings");

                this.appSettings = this.CreateDefaultAppSettings();
                await this.cache.InsertObject(AppSettings.CacheKey, this.appSettings);
            }

            var appVersion = this.GetAppVersion();

            if (appSettings.AppVersion == null || appSettings.AppVersion > appVersion)
            {
                var version = this.appSettings.AppVersion;
                this.appSettings = null;
                throw new IncompatibleAppVersionException(version);
            }

            if (this.appSettings.AppVersion < appVersion)
            {
                await this.MigrateSettingsToLatestVersion();
            }
        }

        return this.appSettings;
    }

    private async Task MigrateSettingsToLatestVersion()
    {
        if (this.appSettings == null)
        {
            return;
        }

        var defaultSettings = this.CreateDefaultAppSettings();

        this.logger.LogInformation(
            "Migrating settings from version {SourceVersion} to {TargetVersion}",
            this.appSettings.AppVersion,
            defaultSettings.AppVersion);

        this.appSettings.AppVersion = defaultSettings.AppVersion;
        this.appSettings.SwitchSettings = defaultSettings.SwitchSettings;

        await this.cache.InsertObject(AppSettings.CacheKey, this.appSettings);
    }

    public async Task SaveAppSettings(AppSettings appSettings)
    {
        this.ThrowIfDisposed();

        this.logger.LogDebug("Saving the app settings");
        await this.cache.InsertObject(AppSettings.CacheKey, appSettings);

        this.appSettings = appSettings;
    }

    public void InvalidateAppSettings()
    {
        this.ThrowIfDisposed();
        this.appSettings = null;
        this.settingsInvalidated.OnNext(Unit.Default);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (!this.Disposed)
        {
            await BlobCache.Shutdown();
            this.settingsInvalidated.Dispose();
            this.Disposed = true;
        }
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
            CharsByKeyboardLayoutId = this.layoutService.GetKeyboardLayouts()
                .ToDictionary(layout => layout.Id, _ => String.Empty),
            InstantSwitching = true,
            SwitchLayout = true,
            ShowUninstalledLayoutsMessage = true,
            AppVersion = this.GetAppVersion()
        };

    private Version GetAppVersion() =>
        Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
}
