using System.Reactive.Concurrency;

using Microsoft.Extensions.Configuration;

using SharpHook;
using SharpHook.Native;

namespace KeyboardSwitch.Linux;

public static class ServiceExtensions
{
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .Configure<StartupSettings>(config.GetSection("Startup"))
            .AddLayoutService()
            .AddSingleton<IClipboardService, XClipboardService>()
            .AddSingleton<IStartupService, FreedesktopStartupService>()
            .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
            .AddSingleton<IUserActivitySimulator>(
                sp => new SharpUserActivitySimulator(
                    sp.GetRequiredService<IEventSimulator>(),
                    sp.GetRequiredService<IScheduler>(),
                    KeyCode.VcLeftControl))
            .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, StartupSetupService>()
            .AddSingleton<IUserProvider, PosixUserProvider>()
            .AddSingleton<IMainLoopRunner, XMainLoopRunner>()
            .AddSingleton<X11Service>();

    private static IServiceCollection AddLayoutService(this IServiceCollection services)
    {
        var currentDesktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        bool isGnome = currentDesktopEnvironment != null &&
            (currentDesktopEnvironment.Contains("gnome", StringComparison.CurrentCultureIgnoreCase) ||
            currentDesktopEnvironment.Contains("unity", StringComparison.CurrentCultureIgnoreCase));

        return isGnome
            ? services.AddSingleton<ILayoutService, GnomeLayoutService>()
            : services.AddSingleton<ILayoutService, XLayoutService>();
    }
}
