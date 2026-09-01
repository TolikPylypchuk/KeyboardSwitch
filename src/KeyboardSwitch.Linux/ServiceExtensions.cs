using KeyboardSwitch.Core.Services.Settings;

using Microsoft.Extensions.Configuration;

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
                .AddLayoutService()
                .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>()
                .AddSingleton<X11Service>();

        private IServiceCollection AddWaylandServices() =>
            services
                .AddSingleton<IClipboardService, WlClipboardService>()
                .AddSingleton<ILayoutService, PlaceholderLayoutService>()
                .AddSingleton<IMainLoopRunner, NoOpMainLoopRunner>()
                .AddSingleton<IAutoConfigurationService, WlAutoConfigurationService>();

        private IServiceCollection AddLayoutService() =>
            GnomeDetector.IsRunningOnGnome()
                ? services.AddSingleton<ILayoutService, GnomeLayoutService>()
                : services.AddSingleton<ILayoutService, XLayoutService>();
    }

    private static bool ShouldUseXsel(IServiceProvider sp) =>
        sp.GetRequiredService<IAppSettingsService>().GetAppSettings().Result.UseXsel;
}
