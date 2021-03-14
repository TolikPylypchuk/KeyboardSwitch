using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.NotSupported;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Linux
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchLinuxServices(this IServiceCollection services) =>
            services.AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>();
    }
}
