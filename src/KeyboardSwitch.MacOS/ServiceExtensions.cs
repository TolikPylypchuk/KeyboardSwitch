namespace KeyboardSwitch.MacOS;

using Microsoft.Extensions.Configuration;

using SharpHook;
using SharpHook.Native;

public static class ServiceExtensions
{
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .Configure<LaunchdSettings>(config.GetSection("Launchd"))
            .AddSingleton<ILayoutService, MacLayoutService>()
            .AddSingleton<IStartupService, LaunchdStartupService>()
            .AddSingleton<IServiceCommunicator, LaunchdServiceCommunicator>()
            .AddSingleton<IUserActivitySimulator>(
                sp => new SharpUserActivitySimulator(sp.GetRequiredService<IEventSimulator>(), KeyCode.VcLeftMeta))
            .AddSingleton<IAutoConfigurationService, MacAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, LaunchdSetupService>()
            .AddSingleton<IMainLoopRunner, MacOSMainLoopRunner>();
}
