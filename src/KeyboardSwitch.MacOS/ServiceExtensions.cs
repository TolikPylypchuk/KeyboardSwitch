namespace KeyboardSwitch.MacOS;

using System.Reactive.Concurrency;

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
            .AddSingleton<IClipboardService, MacClipboardService>()
            .AddSingleton<IStartupService, LaunchdStartupService>()
            .AddSingleton<IServiceCommunicator, LaunchdServiceCommunicator>()
            .AddSingleton<IUserActivitySimulator>(
                sp => new SharpUserActivitySimulator(
                    sp.GetRequiredService<IEventSimulator>(),
                    sp.GetRequiredService<IScheduler>(),
                    KeyCode.VcLeftMeta))
            .AddSingleton<IAutoConfigurationService, MacAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, LaunchdSetupService>()
            .AddSingleton<IUserProvider, PosixUserProvider>()
            .AddSingleton<IMainLoopRunner, MacMainLoopRunner>();
}
