using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services
{
    internal sealed class BlobCacheSettingsService : DisposableService, ISettingsService, IAsyncDisposable
    {
        private readonly IBlobCache cache;
        private readonly ILayoutService layoutService;
        private readonly ILogger<BlobCacheSettingsService> logger;

        private AppSettings? appSettings;

        public BlobCacheSettingsService(
            IBlobCache cache,
            ILayoutService layoutService,
            ILogger<BlobCacheSettingsService> logger)
        {
            this.cache = cache;
            this.layoutService = layoutService;
            this.logger = logger;
        }

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

        public void InvalidateSwitchSettings()
        {
            this.ThrowIfDisposed();
            this.appSettings = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.Disposed)
            {
                await BlobCache.Shutdown();
                this.Disposed = true;
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
                    .ToDictionary(layout => layout.Id, this.GetCharsForLayout),
                InstantSwitching = true,
                SwitchLayout = true,
#if DEBUG
                ServicePath = @"..\..\..\..\..\KeyboardSwitch\bin\x64\Debug\netcoreapp3.1\KeyboardSwitch",
#else
                ServicePath = nameof(KeyboardSwitch),
#endif
            };

        private string GetCharsForLayout(KeyboardLayout layout)
            => layout.Culture.TwoLetterISOLanguageName switch
            {
                "en" => @"qwertyuiop[]\asdfghjkl;'zxcvbnm,./QWERTYUIOP{}|ASDFGHJKL:""ZXCVBNM<>?`1234567890-=~!@#$%^&*()_+",
                "uk" => @"йцукенгшщзхї\фівапролджєячсмитьбю.ЙЦУКЕНГШЩЗХЇ/ФІВАПРОЛДЖЄЯЧСМИТЬБЮ,'1234567890-=₴!""№;%:?*()_+",
                "ru" => @"йцукенгшщзхъ\фывапролджэячсмитьбю.ЙЦУКЕНГШЩЗХЪ/ФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,ё1234567890-=Ё!""№;%:?*()_+",
                "pl" => @"qwertyuiop[]\asdfghjkl;'zxcvbnm,./QWERTYUIOP{}|ASDFGHJKL:""ZXCVBNM<>?`1234567890-=~!@#$%^&*()_+",
                "de" => @"qwertzuiopü+#asdfghjklöäyxcvbnm,.-QWERTZUIOPÜ*'ASDFGHJKLÖÄYXCVBNM;:_^1234567890ß´°!""§$%&/()=?`",
                "fr" => @"azertyuiop^$*qsdfghjklmùwxcvbn,;:!AZERTYUIOP¨£µQSDFGHJKLM%WXCVBN?./§²&é""'(-è_çà)=~1234567890°+",
                "es" => @"qwertyuiop`+çasdfghjklñ´zxcvbnm,.-QWERTYUIOP^*ÇASDFGHJKLÑ¨ZXCVBNM;:_º1234567890'¡ª!""·$%&/()=?¿",
                _ => String.Empty
            };
    }
}
