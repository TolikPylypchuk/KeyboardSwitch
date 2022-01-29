namespace KeyboardSwitch.MacOS;

using Microsoft.Extensions.Configuration;

public static class ServiceExtensions
{
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .Configure<LaunchdSettings>(config.GetSection("Launchd"))
            .AddSingleton<IUserActivitySimulator, MacUserActivitySimulator>()
            .AddSingleton<ILayoutService, MacLayoutService>()
            .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
            .AddSingleton<IStartupService, LaunchdStartupService>()
            .AddSingleton<IServiceCommunicator, LaunchdServiceCommunicator>()
            .AddSingleton<IAutoConfigurationService, MacAutoConfigurationService>()
            .AddSingleton<IInitialSetupService, LaunchdSetupService>();

}
