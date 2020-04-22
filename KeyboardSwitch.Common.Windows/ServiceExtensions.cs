using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using Akavache;
using Akavache.Sqlite3;

using GregsStack.InputSimulatorStandard;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Common.Windows.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Common.Windows
{
    public static class SerivceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchWindowsServices(this IServiceCollection services)
            => services
                .AddSingleton<IInputSimulator>(new InputSimulator())
                .AddSingleton(CreateBlobCache)
                .AddSingleton<IKeysService, KeysService>()
                .AddSingleton<IKeyboardHookService, KeyboardHookService>()
                .AddSingleton<ITextService, ClipboardTextService>()
                .AddSingleton<ISwitchService, SwitchService>()
                .AddSingleton<ILayoutService, LayoutService>();

        private static IBlobCache CreateBlobCache(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SerivceExtensions));

            logger.LogInformation("Spinning up the blob cache");

            BlobCache.ApplicationName = nameof(KeyboardSwitch);

            var options = services.GetRequiredService<IOptions<GlobalSettings>>();
            var scheduler = services.GetService<IScheduler>();

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                options.Value.Path);

            bool shouldCreateDefaultSettings = true;

            if (!File.Exists(path))
            {
                logger.LogInformation("The blob cache file for settings not found");
                logger.LogInformation("Creating the file and putting the default settings into it");

                var file = new FileInfo(path);
                file.Directory.Create();
                file.Create().Close();

                shouldCreateDefaultSettings = true;
            }

            var cache = new SqlRawPersistentBlobCache(path, scheduler);

            if (shouldCreateDefaultSettings)
            {
                var layoutService = services.GetRequiredService<ILayoutService>();
                var settings = CreateDefaultSettings(layoutService);
                cache.InsertObject(SwitchSettings.CacheKey, settings).Wait();
            }

            return cache;
        }

        private static SwitchSettings CreateDefaultSettings(ILayoutService layoutService)
            => new SwitchSettings
            {
                Forward = 'X',
                Backward = 'Z',
                ModifierKeys = new List<ModifierKeys> { ModifierKeys.Alt, ModifierKeys.Ctrl },
                CharsByKeyboardLayoutId = layoutService.GetKeyboardLayouts()
                    .ToDictionary(layout => layout.Id, GetCharsForLayout)
            };

        private static string GetCharsForLayout(KeyboardLayout layout)
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
