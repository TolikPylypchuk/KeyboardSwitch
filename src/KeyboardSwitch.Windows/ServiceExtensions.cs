using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Windows;

public static class SerivceExtensions
{
    extension(IServiceCollection services)
    {
        [SuppressMessage(
            "Style",
            "IDE0060:Remove unused parameter",
            Justification = "macOS and Linux need the config and these methods must be the same across all platrofms")]
        public IServiceCollection AddNativeKeyboardSwitchServices(IConfiguration config) =>
            services
                .AddSingleton(SimulationModifierKeyCodeProvider.Control)
                .AddSingleton<ILayoutService, WinLayoutService>()
                .AddSingleton<IClipboardService, WinClipboardService>()
                .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
                .AddSingleton<IStartupService, RegistryStartupService>()
                .AddSingleton<IAutoConfigurationService, WinAutoConfigurationService>()
                .AddSingleton<IInitialSetupService, StartupSetupService>()
                .AddSingleton<IUserProvider, WinUserProvider>()
                .AddSingleton<IMainLoopRunner, NoOpMainLoopRunner>();
    }
}
