using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Linux.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Linux
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddNativeKeyboardSwitchServices(this IServiceCollection services) =>
            services
                .AddSingleton<ITextService, ClipboardTextService>()
                .AddSingleton<IUserActivitySimulator, XUserActivitySimulator>()
                .AddSingleton<ILayoutService, XLayoutService>()
                .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
                .AddSingleton<IStartupService, CronStartupService>()
                .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>();
    }
}
