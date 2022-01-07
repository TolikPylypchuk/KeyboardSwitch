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
            .AddSingleton<ILayoutService, XLayoutService>()
            .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
            .AddSingleton<IStartupService, FreedesktopStartupService>()
            .AddSingleton<IAutoConfigurationService, XAutoConfigurationService>();
}
