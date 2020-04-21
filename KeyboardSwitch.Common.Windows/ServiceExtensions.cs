using System;
using System.Collections.Generic;
using System.IO;
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

            logger.LogInformation("Spinning the blob cache up");

            BlobCache.ApplicationName = "KeyboardSwitch";

            var options = services.GetRequiredService<IOptions<GlobalSettings>>();
            var scheduler = services.GetService<IScheduler>();

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                options.Value.Path);

            bool shouldCreateDefaultSettings = true;

            if (!File.Exists(path))
            {
                logger.LogInformation("The blob cache file for settings doesn't exist");
                logger.LogInformation("Creating the file and putting the default settings into it");

                var file = new FileInfo(path);
                file.Directory.Create();
                file.Create().Close();

                shouldCreateDefaultSettings = true;
            }

            var cache = new SqlRawPersistentBlobCache(path, scheduler);

            if (shouldCreateDefaultSettings)
            {
                cache.InsertObject(SwitchSettings.CacheKey, CreateDefaultSettings()).Wait();
            }

            return cache;
        }

        private static SwitchSettings CreateDefaultSettings()
            => new SwitchSettings
            {
                Forward = 'X',
                Backward = 'Z',
                ModifierKeys = new List<ModifierKeys> { ModifierKeys.Alt, ModifierKeys.Ctrl }
            };
    }
}
