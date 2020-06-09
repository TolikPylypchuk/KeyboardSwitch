using System;
using System.IO;
using System.Reactive.Concurrency;

using Akavache;
using Akavache.Sqlite3;

using GregsStack.InputSimulatorStandard;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Windows.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Windows
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
                .AddSingleton<LayoutService>()
                .AddSingleton<ILayoutService>(provider => provider.GetRequiredService<LayoutService>())
                .AddSingleton<ILayoutLoaderSrevice>(provider => provider.GetRequiredService<LayoutService>())
                .AddSingleton<IAutoConfigurationService, AutoConfigurationService>();

        private static IBlobCache CreateBlobCache(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SerivceExtensions));

            logger.LogInformation("Spinning up the blob cache");

            var options = services.GetRequiredService<IOptions<GlobalSettings>>();
            var scheduler = services.GetService<IScheduler>();

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                options.Value.Path);

            if (!File.Exists(path))
            {
                logger.LogInformation("The blob cache file for settings not found - creating a new file");

                var file = new FileInfo(path);
                file.Directory.Create();
                file.Create().Close();
            }

            return new SqlRawPersistentBlobCache(path, scheduler);
        }
    }
}
