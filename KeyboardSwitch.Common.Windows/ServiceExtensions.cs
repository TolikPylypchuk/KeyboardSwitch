using System;
using System.IO;
using System.Reactive.Concurrency;

using Akavache;
using Akavache.Sqlite3;

using GregsStack.InputSimulatorStandard;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Common.Windows.Services;

using Microsoft.Extensions.DependencyInjection;
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
                .AddSingleton<ISwitchService, SwitchService>();

        private static IBlobCache CreateBlobCache(IServiceProvider services)
        {
            BlobCache.ApplicationName = "KeyboardSwitch";

            var options = services.GetRequiredService<IOptions<GlobalSettings>>();
            var scheduler = services.GetService<IScheduler>();

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                options.Value.Path);

            return new SQLitePersistentBlobCache(path, scheduler);
        }
    }
}
