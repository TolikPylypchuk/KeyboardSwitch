using System;

using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Core.Settings;
using KeyboardSwitch.Linux.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Linux
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddNativeKeyboardSwitchServices(this IServiceCollection services) =>
            services
                .AddSingleton(ServiceCommunicator)
                .AddSingleton<ITextService, ClipboardTextService>()
                .AddSingleton<IUserActivitySimulator, XUserActivitySimulator>()
                .AddSingleton<ILayoutService, XLayoutService>()
                .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
                .AddSingleton<IStartupService, SystemdStartupService>()
                .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>();

        private static IServiceCommunicator ServiceCommunicator(IServiceProvider services)
        {
            var directCommunicator = ActivatorUtilities.CreateInstance<DirectServiceCommunicator>(services);
            return new SystemdServiceCommunicator(
                directCommunicator, services.GetRequiredService<IOptions<GlobalSettings>>());
        }
    }
}
