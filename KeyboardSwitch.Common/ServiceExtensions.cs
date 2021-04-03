using System.Reactive.Concurrency;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TextCopy;

namespace KeyboardSwitch.Common
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchServices(this IServiceCollection services) =>
            services
                .AddSingleton<IKeyboardHookService, UioHookService>()
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
                .AddSingleton<IScheduler>(DefaultScheduler.Instance);

        public static IServiceCollection AddClipboard(this IServiceCollection services)
        {
            services.InjectClipboard();
            return services;
        }
    }
}
