using KeyboardSwitch.Core.Services.Settings;

using Microsoft.Extensions.Configuration;

namespace KeyboardSwitch.Linux;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNativeKeyboardSwitchServices(IConfiguration config)
        {
            var desktopEnvironment = SessionDetector.CurrentDesktopEnvironment;

            services
                .Configure<StartupSettings>(config.GetSection("Startup"))
                .AddSingleton(SimulationModifierKeyCodeProvider.Control)
                .AddSingleton<IStartupService, FreedesktopStartupService>()
                .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
                .AddSingleton<IInitialSetupService, StartupSetupService>()
                .AddSingleton<IUserProvider, PosixUserProvider>()
                .AddSingleton<DBusConnectionProvider>()
                .AddLockStateProvider(desktopEnvironment);

            return SessionDetector.IsRunningOnWayland
                ? services.AddWaylandServices(desktopEnvironment)
                : services.AddX11Services(desktopEnvironment);
        }

        private IServiceCollection AddX11Services(DesktopEnvironment desktopEnvironment) =>
            services
                .AddSingleton<IMainLoopRunner>(sp => ShouldUseXsel(sp)
                    ? ActivatorUtilities.CreateInstance<NoOpMainLoopRunner>(sp)
                    : ActivatorUtilities.CreateInstance<XMainLoopRunner>(sp))
                .AddSingleton<IClipboardService>(sp => ShouldUseXsel(sp)
                    ? ActivatorUtilities.CreateInstance<XselClipboardService>(sp)
                    : ActivatorUtilities.CreateInstance<XClipboardService>(sp))
                .AddX11LayoutService(desktopEnvironment)
                .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>()
                .AddSingleton<X11Service>();

        private IServiceCollection AddWaylandServices(DesktopEnvironment desktopEnvironment) =>
            services
                .AddSingleton<IClipboardService, WlClipboardService>()
                .AddWaylandLayoutService(desktopEnvironment)
                .AddSingleton<IMainLoopRunner, NoOpMainLoopRunner>()
                .AddSingleton<IAutoConfigurationService, XkbAutoConfigurationService>();

        private IServiceCollection AddX11LayoutService(DesktopEnvironment desktopEnvironment) =>
            desktopEnvironment switch
            {
                DesktopEnvironment.Gnome => services
                    .AddSingleton<GnomeShellExtensionClient>()
                    .AddSingleton<XLayoutService>()
                    .AddSingleton<ILayoutService>(sp =>
                        CreateGnomeLayoutService(sp, sp.GetRequiredService<XLayoutService>())),

                _ => services.AddSingleton<ILayoutService, XLayoutService>()
            };

        private IServiceCollection AddWaylandLayoutService(DesktopEnvironment desktopEnvironment) =>
            desktopEnvironment switch
            {
                DesktopEnvironment.Gnome => services
                    .AddSingleton<GnomeShellExtensionClient>()
                    .AddSingleton<ILayoutService>(sp => CreateGnomeLayoutService(sp, null)),

                DesktopEnvironment.Kde => services
                    .AddSingleton<KdeKeyboardLayoutsClient>()
                    .AddSingleton<KxkbConfigReader>()
                    .AddSingleton<ILayoutService, KdeLayoutService>(),

                _ => services.AddSingleton<ILayoutService, PlaceholderLayoutService>()
            };

        private IServiceCollection AddLockStateProvider(DesktopEnvironment desktopEnvironment) =>
            services
                .AddSingleton<ILockStateClient, LogindSessionClient>()
                .AddScreenSaverClient(desktopEnvironment)
                .AddSingleton<ILockStateProvider, DBusLockStateProvider>();

        private IServiceCollection AddScreenSaverClient(DesktopEnvironment desktopEnvironment) =>
            desktopEnvironment switch
            {
                DesktopEnvironment.Gnome => services
                    .AddSingleton(ScreenSaverEndpoint.Gnome)
                    .AddSingleton<ILockStateClient, ScreenSaverClient>(),

                DesktopEnvironment.Kde => services
                    .AddSingleton(ScreenSaverEndpoint.Freedesktop)
                    .AddSingleton<ILockStateClient, ScreenSaverClient>(),

                _ => services
            };
    }

    private static bool ShouldUseXsel(IServiceProvider sp) =>
        sp.GetRequiredService<IAppSettingsService>().GetAppSettings().Result.UseXsel;

    private static GnomeLayoutService CreateGnomeLayoutService(IServiceProvider sp, ILayoutService? fallback) =>
        new(
            sp.GetRequiredService<GnomeShellExtensionClient>(),
            fallback,
            sp.GetRequiredService<ILogger<GnomeLayoutService>>());
}
