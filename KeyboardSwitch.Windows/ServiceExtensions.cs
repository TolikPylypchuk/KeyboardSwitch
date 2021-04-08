using GregsStack.InputSimulatorStandard;

using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Windows.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Windows
{
    public static class SerivceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchWindowsServices(this IServiceCollection services)
            => services
                .AddSingleton<IInputSimulator>(new InputSimulator())
                .AddSingleton<IUserActivitySimulator, UserActivitySimulator>()
                .AddSingleton<LayoutService>()
                .AddSingleton<ILayoutService>(provider => provider.GetRequiredService<LayoutService>())
                .AddSingleton<ILayoutLoaderSrevice>(provider => provider.GetRequiredService<LayoutService>())
                .AddSingleton<IStartupService, RegistryStartupService>()
                .AddSingleton<IAutoConfigurationService, AutoConfigurationService>();
    }
}
