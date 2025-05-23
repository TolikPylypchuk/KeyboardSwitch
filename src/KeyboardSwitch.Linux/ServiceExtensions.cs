using System.Reactive.Concurrency;

using KeyboardSwitch.Core.Services.Settings;

using Microsoft.Extensions.Configuration;

using SharpHook;
using SharpHook.Data;

namespace KeyboardSwitch.Linux;

public static class ServiceExtensions
{
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .Configure<StartupSettings>(config.GetSection("Startup"))
            .AddLayoutService()
            .AddSingleton<IClipboardService>(CreateClipboardService)
            .AddSingleton<IStartupService, FreedesktopStartupService>()
            .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
            .AddSingleton<IUserActivitySimulator>(
                sp => new SharpUserActivitySimulator(
                    sp.GetRequiredService<IEventSimulator>(),
                    sp.GetRequiredService<IScheduler>(),
                    KeyCode.VcLeftControl))
            .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, LinuxSetupService>()
            .AddSingleton<IUserProvider, PosixUserProvider>()
            .AddSingleton<IMainLoopRunner, XMainLoopRunner>()
            .AddSingleton<X11Service>();

    private static IServiceCollection AddLayoutService(this IServiceCollection services) =>
        GnomeDetector.IsRunningOnGnome()
            ? services.AddSingleton<ILayoutService, GnomeLayoutService>()
            : services.AddSingleton<ILayoutService, XLayoutService>();

    private static IClipboardService CreateClipboardService(this IServiceProvider sp)
    {
        var settingsService = sp.GetRequiredService<IAppSettingsService>();
        var settings = settingsService.GetAppSettings().Result;

        return settings.UseXsel
            ? ActivatorUtilities.CreateInstance<XselClipboardService>(sp)
            : ActivatorUtilities.CreateInstance<XClipboardService>(sp);
    }
}
