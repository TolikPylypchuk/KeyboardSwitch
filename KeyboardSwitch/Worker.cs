using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly IKeyboardHookService keyboardHookService;
        private readonly ISwitchService switchService;
        private readonly ILayoutService layoutService;
        private readonly ISettingsService settingsService;
        private readonly IKeysService keysService;
        private readonly IBlobCache blobCache;
        private readonly ILogger<Worker> logger;

        public Worker(
            IKeyboardHookService keyboardHookService,
            ISwitchService switchService,
            ILayoutService layoutService,
            ISettingsService settingsService,
            IKeysService keysService,
            IBlobCache blobCache,
            ILogger<Worker> logger)
        {
            this.keyboardHookService = keyboardHookService;
            this.switchService = switchService;
            this.layoutService = layoutService;
            this.settingsService = settingsService;
            this.keysService = keysService;
            this.blobCache = blobCache;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            this.logger.LogTrace("Configuring the keyboard switch service");

            await this.CreateDefaultSettingsIfNeededAsync();
            await this.RegisterHotKeysAsync();

            this.logger.LogTrace("Starting the service execution");

            await this.keyboardHookService.WaitForMessagesAsync(token);
        }

        private async Task RegisterHotKeysAsync()
        {
            this.logger.LogTrace("Registering hot keys to switch forward and backward");

            var settings = await this.settingsService.GetSwitchSettingsAsync();

            var modifiers = settings.ModifierKeys.Flatten();
            int forwardKeyCode = this.keysService.GetVirtualKeyCode(settings.Forward);
            int backwardKeyCode = this.keysService.GetVirtualKeyCode(settings.Backward);

            this.keyboardHookService.RegisterHotKey(modifiers, forwardKeyCode);
            this.keyboardHookService.RegisterHotKey(modifiers, backwardKeyCode);

            this.keyboardHookService.HotKeyPressed
                .Select(hotKey => hotKey.VirtualKeyCode == forwardKeyCode
                    ? SwitchDirection.Forward
                    : SwitchDirection.Backward)
                .SubscribeAsync(this.switchService.SwitchTextAsync);
        }

        private async Task CreateDefaultSettingsIfNeededAsync()
        {
            if (!await this.blobCache.ContainsKey(SwitchSettings.CacheKey))
            {
                this.logger.LogInformation("Settings not found - creating default settings");
                await this.blobCache.InsertObject(SwitchSettings.CacheKey, this.CreateDefaultSettings());
            }
        }

        private SwitchSettings CreateDefaultSettings()
            => new SwitchSettings
            {
                Forward = 'X',
                Backward = 'Z',
                ModifierKeys = new List<ModifierKeys> { ModifierKeys.Alt, ModifierKeys.Ctrl },
                CharsByKeyboardLayoutId = this.layoutService.GetKeyboardLayouts()
                    .ToDictionary(layout => layout.Id, this.GetCharsForLayout),
                InstantSwitching = true,
                SwitchLayout = true
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
