using GregsStack.InputSimulatorStandard;

using KeyboardSwitch.Core.Services.AutoConfiguration;
using KeyboardSwitch.Core.Services.Infrastructure;
using KeyboardSwitch.Core.Services.Layout;
using KeyboardSwitch.Core.Services.Simulation;
using KeyboardSwitch.Core.Services.Startup;
using KeyboardSwitch.Windows.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Windows
{
    public static class SerivceExtensions
    {
        public static IServiceCollection AddNativeKeyboardSwitchServices(this IServiceCollection services) =>
            services
                .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
                .AddSingleton<IKeyboardSimulator>(new KeyboardSimulator())
                .AddSingleton<IUserActivitySimulator, WinUserActivitySimulator>()
                .AddSingleton<WinLayoutService>()
                .AddSingleton<ILayoutService>(provider => provider.GetRequiredService<WinLayoutService>())
                .AddSingleton<ILayoutLoaderSrevice>(provider => provider.GetRequiredService<WinLayoutService>())
                .AddSingleton<IStartupService, RegistryStartupService>()
                .AddSingleton<IAutoConfigurationService, WinAutoConfigurationService>();
    }
}
