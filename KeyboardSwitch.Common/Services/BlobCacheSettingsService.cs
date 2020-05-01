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

        private SwitchSettings? switchSettings;

        public BlobCacheSettingsService(
            IBlobCache cache,
            ILayoutService layoutService,
            ILogger<BlobCacheSettingsService> logger)
        {
            this.cache = cache;
            this.layoutService = layoutService;
            this.logger = logger;
        }

        public async ValueTask<SwitchSettings> GetSwitchSettingsAsync()
        {
            this.ThrowIfDisposed();

            if (this.switchSettings == null)
            {
                this.logger.LogDebug("Getting the switch settings");

                if (await this.cache.ContainsKey(SwitchSettings.CacheKey))
                {
                    this.switchSettings = await this.cache.GetObject<SwitchSettings>(SwitchSettings.CacheKey);
                } else
                {
                    this.logger.LogInformation("Switch settings not found - creating default settings");

                    this.switchSettings = this.CreateDefaultSwitchSettings();
                    await this.cache.InsertObject(SwitchSettings.CacheKey, this.switchSettings);
                }
            }

            return this.switchSettings;
        }

        public async Task SaveSwitchSettingsAsync(SwitchSettings switchSettings)
        {
            this.ThrowIfDisposed();

            this.logger.LogDebug("Saving the switch settings");
            await this.cache.InsertObject(SwitchSettings.CacheKey, switchSettings);

            this.switchSettings = switchSettings;
        }

        public void InvalidateSwitchSettings()
        {
            this.ThrowIfDisposed();
            this.switchSettings = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.Disposed)
            {
                await BlobCache.Shutdown();
                this.Disposed = true;
            }
        }

        private SwitchSettings CreateDefaultSwitchSettings()
            => new SwitchSettings
            {
                Forward = 'X',
                Backward = 'Z',
                ModifierKeys = ModifierKeys.Ctrl | ModifierKeys.Shift,
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
