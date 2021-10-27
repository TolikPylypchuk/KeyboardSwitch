using System.Reactive.Concurrency;

using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Core.Services.Hook;
using KeyboardSwitch.Core.Services.Infrastructure;
using KeyboardSwitch.Core.Services.Settings;
using KeyboardSwitch.Core.Services.Switching;
using KeyboardSwitch.Core.Services.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SharpHook;
using SharpHook.Reactive;

using TextCopy;

namespace KeyboardSwitch.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCoreKeyboardSwitchServices(this IServiceCollection services) =>
            services
                .AddSingleton<IReactiveGlobalHook>(services => new ReactiveGlobalHookAdapter(
                    new TaskPoolGlobalHook(TaskPoolGlobalHookOptions.Sequential)))
                .AddSingleton<IKeyboardHookService, SharpHookService>()
                .AddSingleton<ITextService, ClipboardTextService>()
                .AddClipboard()
                .AddSingleton(BlobCacheFactory.CreateBlobCache)
                .AddSingleton<BlobCacheSettingsService>()
                .AddSingleton<IAppSettingsService>(provider =>
                    provider.GetRequiredService<BlobCacheSettingsService>())
                .AddSingleton<IConverterSettingsService>(provider =>
                    provider.GetRequiredService<BlobCacheSettingsService>())
                .AddSingleton<ISwitchService, SwitchService>()
                .AddSingleton<ServiceProvider<INamedPipeService>>(s => name =>
                    new NamedPipeService(s.GetRequiredService<ILogger<NamedPipeService>>(), name))
                .AddSingleton<ServiceProvider<ISingleInstanceService>>(s => name =>
                    new SingleInstanceService(
                        s.GetRequiredService<ServiceProvider<INamedPipeService>>(),
                        s.GetRequiredService<ILogger<SingleInstanceService>>(),
                        name))
                .AddSingleton<IScheduler>(Scheduler.Default);

        public static IServiceCollection AddClipboard(this IServiceCollection services)
        {
            services.InjectClipboard();
            return services;
        }
    }
}
