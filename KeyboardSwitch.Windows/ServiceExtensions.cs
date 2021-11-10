namespace KeyboardSwitch.Windows;

using Microsoft.Extensions.DependencyInjection;

public static class SerivceExtensions
{
    public static IServiceCollection AddNativeKeyboardSwitchServices(this IServiceCollection services) =>
        services
            .AddSingleton<IServiceCommunicator, DirectServiceCommunicator>()
            .AddSingleton<IKeyboardSimulator>(new KeyboardSimulator())
            .AddSingleton<IUserActivitySimulator, WinUserActivitySimulator>()
            .AddSingleton<WinLayoutService>()
            .AddSingleton<ILayoutService>(provider => provider.GetRequiredService<WinLayoutService>())
            .AddSingleton<ILayoutLoaderSrevice>(provider => provider.GetRequiredService<WinLayoutService>())
            .AddSingleton<IStartupService, RegistryStartupService>()
            .AddSingleton<IAutoConfigurationService, WinAutoConfigurationService>();
}
