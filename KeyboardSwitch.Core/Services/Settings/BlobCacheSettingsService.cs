using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Core.Services.Layout;
using KeyboardSwitch.Core.Settings;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Core.Services.Settings
{
    internal sealed class BlobCacheSettingsService : AsyncDisposable, IAppSettingsService, IConverterSettingsService
    {
        private readonly IBlobCache cache;
        private readonly ILayoutService layoutService;
        private readonly ILogger<BlobCacheSettingsService> logger;
        private readonly Subject<Unit> settingsInvalidated = new();

        private AppSettings? appSettings;
        private ConverterSettings? converterSettings;

        public BlobCacheSettingsService(
            IBlobCache cache,
            ILayoutService layoutService,
            ILogger<BlobCacheSettingsService> logger)
        {
            this.cache = cache;
            this.layoutService = layoutService;
            this.logger = logger;
        }

        public IObservable<Unit> SettingsInvalidated =>
            this.settingsInvalidated.AsObservable();

        public async Task<AppSettings> GetAppSettingsAsync()
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
                $"Migrating settings from version {this.appSettings.AppVersion} to {defaultSettings.AppVersion}");

            this.appSettings.AppVersion = defaultSettings.AppVersion;
            this.appSettings.SwitchSettings = defaultSettings.SwitchSettings;
            this.appSettings.ShowConverter = defaultSettings.ShowConverter;

            await this.cache.InsertObject(AppSettings.CacheKey, this.appSettings);
        }

        public async Task SaveAppSettingsAsync(AppSettings appSettings)
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

        public async ValueTask<ConverterSettings> GetConverterSettingsAsync()
        {
            this.ThrowIfDisposed();

            if (this.converterSettings == null)
            {
                this.logger.LogDebug("Getting the converter settings");

                if (await this.cache.ContainsKey(ConverterSettings.CacheKey))
                {
                    this.converterSettings = await this.cache.GetObject<ConverterSettings>(ConverterSettings.CacheKey);
                } else
                {
                    this.logger.LogInformation("Converter settings not found - creating default settings");

                    this.converterSettings = new ConverterSettings();
                    await this.cache.InsertObject(ConverterSettings.CacheKey, this.converterSettings);
                }
            }

            return this.converterSettings;
        }

        public async Task SaveConverterSettingsAsync(ConverterSettings converterSettings)
        {
            this.ThrowIfDisposed();

            this.logger.LogDebug("Saving the converter settings");
            await this.cache.InsertObject(ConverterSettings.CacheKey, converterSettings);

            this.converterSettings = converterSettings;
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
                    ForwardModifierKeys = new() { ModifierKey.Ctrl, ModifierKey.Shift, ModifierKey.None },
                    BackwardModifierKeys = new() { ModifierKey.Ctrl, ModifierKey.Shift, ModifierKey.Alt },
                    PressCount = 2,
                    WaitMilliseconds = 400,
                    LayoutForwardKeys = new() { KeyCode.VcLeftMeta, KeyCode.VcSpace },
                    LayoutBackwardKeys = new() { KeyCode.VcLeftMeta, KeyCode.VcLeftShift, KeyCode.VcSpace }
                },
                CharsByKeyboardLayoutId = this.layoutService.GetKeyboardLayouts()
                    .ToDictionary(layout => layout.Id, _ => String.Empty),
                InstantSwitching = true,
                SwitchLayout = true,
                ShowUninstalledLayoutsMessage = true,
                ShowConverter = false,
                AppVersion = this.GetAppVersion(),
                ServicePath = nameof(KeyboardSwitch)
            };

        private Version GetAppVersion() =>
            Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
    }
}
