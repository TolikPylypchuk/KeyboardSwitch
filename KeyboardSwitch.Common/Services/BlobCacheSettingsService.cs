using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services
{
    internal sealed class BlobCacheSettingsService
        : DisposableService, IAppSettingsService, IConverterSettingsService, IAsyncDisposable
    {
        private readonly IBlobCache cache;
        private readonly ILayoutService layoutService;
        private readonly ILogger<BlobCacheSettingsService> logger;
        private readonly Subject<Unit> settingsInvalidated = new Subject<Unit>();

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

        public IObservable<Unit> SettingsInvalidated
            => this.settingsInvalidated.AsObservable();

        public async ValueTask<AppSettings> GetAppSettingsAsync()
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
            }

            return this.appSettings;
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

        public async ValueTask DisposeAsync()
        {
            if (!this.Disposed)
            {
                await BlobCache.Shutdown();
                this.Disposed = true;
                this.settingsInvalidated.OnCompleted();
            }
        }

        private AppSettings CreateDefaultAppSettings()
            => new AppSettings
            {
                HotKeySwitchSettings = new HotKeySwitchSettings
                {
                    Forward = 'X',
                    Backward = 'Z',
                    ModifierKeys = ModifierKeys.Ctrl | ModifierKeys.Shift
                },
                ModifierKeysSwitchSettings = new ModifierKeysSwitchSettings
                {
                    ForwardModifierKeys = ModifierKeys.Ctrl | ModifierKeys.Shift,
                    BackwardModifierKeys = ModifierKeys.Alt | ModifierKeys.Ctrl | ModifierKeys.Shift,
                    PressCount = 1,
                    WaitMilliseconds = 300
                },
                SwitchMode = SwitchMode.ModifierKey,
                CharsByKeyboardLayoutId = this.layoutService.GetKeyboardLayouts()
                    .ToDictionary(layout => layout.Id, _ => String.Empty),
                InstantSwitching = true,
                SwitchLayout = true,
                ShowUninstalledLayoutsMessage = true,
                ServicePath = nameof(KeyboardSwitch)
            };
    }
}
