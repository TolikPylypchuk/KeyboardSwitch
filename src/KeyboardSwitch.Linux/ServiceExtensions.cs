using KeyboardSwitch.Core.Services.Settings;

using Microsoft.Extensions.Configuration;

using Tmds.DBus.Protocol;

namespace KeyboardSwitch.Linux;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNativeKeyboardSwitchServices(IConfiguration config)
        {
            services
                .Configure<StartupSettings>(config.GetSection("Startup"))
                .AddSingleton(SimulationModifierKeyCodeProvider.Control)
                .AddSingleton<IStartupService, FreedesktopStartupService>()
                .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
                .AddSingleton<IInitialSetupService, LinuxSetupService>()
                .AddSingleton<IUserProvider, PosixUserProvider>();

            return LinuxSessionDetector.IsRunningOnWayland
                ? services.AddWaylandServices()
                : services.AddX11Services();
        }

        private IServiceCollection AddX11Services() =>
            services
                .AddSingleton<IMainLoopRunner>(sp => ShouldUseXsel(sp)
                    ? ActivatorUtilities.CreateInstance<NoOpMainLoopRunner>(sp)
                    : ActivatorUtilities.CreateInstance<XMainLoopRunner>(sp))
                .AddSingleton<IClipboardService>(sp => ShouldUseXsel(sp)
                    ? ActivatorUtilities.CreateInstance<XselClipboardService>(sp)
                    : ActivatorUtilities.CreateInstance<XClipboardService>(sp))
                .AddX11LayoutService()
                .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>()
                .AddSingleton<X11Service>();

        private IServiceCollection AddWaylandServices() =>
            services
                .AddSingleton<IClipboardService, WlClipboardService>()
                .AddWaylandLayoutService()
                .AddSingleton<IMainLoopRunner, NoOpMainLoopRunner>()
                .AddSingleton<IAutoConfigurationService, WlAutoConfigurationService>();

        private IServiceCollection AddX11LayoutService() =>
            GnomeDetector.IsRunningOnGnome()
                ? services
                    .AddSingleton(DBusConnection.Session)
                    .AddSingleton<GnomeShellExtensionClient>()
                    .AddSingleton<XLayoutService>()
                    .AddSingleton<ILayoutService>(sp =>
                        CreateGnomeLayoutService(sp, sp.GetRequiredService<XLayoutService>()))
                : services.AddSingleton<ILayoutService, XLayoutService>();

        private IServiceCollection AddWaylandLayoutService() =>
            GnomeDetector.IsRunningOnGnome()
                ? services
                    .AddSingleton(DBusConnection.Session)
                    .AddSingleton<GnomeShellExtensionClient>()
                    .AddSingleton<ILayoutService>(sp => CreateGnomeLayoutService(sp, null))
                : services.AddSingleton<ILayoutService, PlaceholderLayoutService>();
    }

    private static bool ShouldUseXsel(IServiceProvider sp) =>
        sp.GetRequiredService<IAppSettingsService>().GetAppSettings().Result.UseXsel;

    private static GnomeLayoutService CreateGnomeLayoutService(IServiceProvider sp, ILayoutService? fallback) =>
        new(
            sp.GetRequiredService<GnomeShellExtensionClient>(),
            fallback,
            sp.GetRequiredService<ILogger<GnomeLayoutService>>());
}
