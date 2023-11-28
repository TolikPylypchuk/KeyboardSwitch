namespace KeyboardSwitch.Linux;

using Microsoft.Extensions.Configuration;

using SharpHook;
using SharpHook.Native;

public static class ServiceExtensions
{
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .Configure<StartupSettings>(config.GetSection("Startup"))
            .AddLayoutService()
            .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
            .AddSingleton<IStartupService, FreedesktopStartupService>()
            .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
            .AddSingleton<IUserActivitySimulator>(
                sp => new SharpUserActivitySimulator(sp.GetRequiredService<IEventSimulator>(), KeyCode.VcLeftControl))
            .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, StartupSetupService>()
            .AddSingleton<IMainLoopRunner, NoOpMainLoopRunner>();

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
