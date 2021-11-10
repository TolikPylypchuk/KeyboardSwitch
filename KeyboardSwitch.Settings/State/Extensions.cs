namespace KeyboardSwitch.Settings.State;

using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddSuspensionDriver(this IServiceCollection services) =>
        services.AddSingleton<ISuspensionDriver, AkavacheSuspensionDriver<AppState>>();
}
