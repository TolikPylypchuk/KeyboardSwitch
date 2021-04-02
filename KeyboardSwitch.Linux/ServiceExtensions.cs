using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.NotSupported;
using KeyboardSwitch.Linux.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Linux
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchLinuxServices(this IServiceCollection services) =>
            services
                .AddSingleton<ITextService, ClipboardTextService>()
                .AddSingleton<ILayoutService, X11LayoutService>()
                .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
                .AddSingleton<IStartupService, StartupService>()
                .AddSingleton<IAutoConfigurationService, AutoConfigurationService>();
    }
}
