namespace KeyboardSwitch.Windows;

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            .AddSingleton<WinLayoutService>()
            .AddSingleton<ILayoutService>(provider => provider.GetRequiredService<WinLayoutService>())
            .AddSingleton<ILayoutLoaderSrevice>(provider => provider.GetRequiredService<WinLayoutService>())
            .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
            .AddSingleton<IUserActivitySimulator, SharpUserActivitySimulator>()
            .AddSingleton<IStartupService, RegistryStartupService>()
            .AddSingleton<IAutoConfigurationService, WinAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, StartupSetupService>()
            .AddSingleton<IMainLoopRunner, NoOpMainLoopRunner>();
}
