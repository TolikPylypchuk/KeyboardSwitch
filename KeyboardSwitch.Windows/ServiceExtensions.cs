using GregsStack.InputSimulatorStandard;

using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Windows.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Windows
{
    public static class SerivceExtensions
    {
        public static IServiceCollection AddNativeKeyboardSwitchServices(this IServiceCollection services) =>
            services
                .AddSingleton<IKeyboardSimulator>(new KeyboardSimulator())
                .AddSingleton<IUserActivitySimulator, UserActivitySimulator>()
                .AddSingleton<LayoutService>()
                .AddSingleton<ILayoutService>(provider => provider.GetRequiredService<LayoutService>())
                .AddSingleton<ILayoutLoaderSrevice>(provider => provider.GetRequiredService<LayoutService>())
                .AddSingleton<IStartupService, RegistryStartupService>()
                .AddSingleton<IAutoConfigurationService, AutoConfigurationService>();
    }
}
