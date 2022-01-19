namespace KeyboardSwitch.MacOS;

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;

public static class ServiceExtensions
{
    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Linux needs the configuration and these methods must be the same across all platrofms")]
    public static IServiceCollection AddNativeKeyboardSwitchServices(
        this IServiceCollection services,
        IConfiguration config) =>
        services
            .AddSingleton<IUserActivitySimulator, MacUserActivitySimulator>()
            .AddSingleton<ILayoutService, MacLayoutService>()
            .AddSingleton<ILayoutLoaderSrevice, NotSupportedLayoutLoaderService>()
            .AddSingleton<IStartupService, LaunchdStartupService>()
            .AddSingleton<IAutoConfigurationService, MacAutoConfigurationService>();

}
