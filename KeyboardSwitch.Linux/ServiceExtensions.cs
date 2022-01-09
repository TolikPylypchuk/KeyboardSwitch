namespace KeyboardSwitch.Linux;

using Microsoft.Extensions.Configuration;

public static class ServiceExtensions
{
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .Configure<StartupSettings>(config.GetSection("Startup"))
            .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
            .AddSingleton<ITextService, ClipboardTextService>()
            .AddLayoutService()
            .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
            .AddSingleton<IStartupService, FreedesktopStartupService>()
            .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>();

    private static IServiceCollection AddLayoutService(this IServiceCollection services)
    {
        var currentDesktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        bool isGnome = currentDesktopEnvironment != null &&
            (currentDesktopEnvironment.ToLower().Contains("gnome") ||
            currentDesktopEnvironment.ToLower().Contains("unity"));

        return isGnome
            ? services.AddSingleton<ILayoutService, GnomeLayoutService>()
            : services.AddSingleton<ILayoutService, XLayoutService>();
    }
}
