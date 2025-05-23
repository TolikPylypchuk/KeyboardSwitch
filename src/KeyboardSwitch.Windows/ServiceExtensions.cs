using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharpHook;
using SharpHook.Data;

namespace KeyboardSwitch.Windows;

public static class SerivceExtensions
{
    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "macOS and Linux need the config and these methods must be the same across all platrofms")]
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .AddSingleton<ILayoutService, WinLayoutService>()
            .AddSingleton<IClipboardService, WinClipboardService>()
            .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
            .AddSingleton<IUserActivitySimulator>(
                sp => new SharpUserActivitySimulator(
                    sp.GetRequiredService<IEventSimulator>(),
                    sp.GetRequiredService<IScheduler>(),
                    KeyCode.VcLeftControl))
            .AddSingleton<IStartupService, RegistryStartupService>()
            .AddSingleton<IAutoConfigurationService, WinAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, StartupSetupService>()
            .AddSingleton<IUserProvider, WinUserProvider>()
            .AddSingleton<IMainLoopRunner, NoOpMainLoopRunner>();
}
