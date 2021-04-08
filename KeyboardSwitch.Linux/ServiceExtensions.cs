using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Linux.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Linux
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchLinuxServices(this IServiceCollection services) =>
            services
                .AddSingleton<ITextService, ClipboardTextService>()
                .AddSingleton<IUserActivitySimulator, NoOpSimulator>()
                .AddSingleton<ILayoutService, X11LayoutService>()
                .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
                .AddSingleton<IStartupService, StartupService>()
                .AddSingleton<IAutoConfigurationService, AutoConfigurationService>();
    }
}
