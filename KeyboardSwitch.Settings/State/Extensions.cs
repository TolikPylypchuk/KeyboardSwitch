using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

namespace KeyboardSwitch.Settings.State
{
    public static class Extensions
    {
        public static IServiceCollection AddSuspensionDriver(this IServiceCollection services) =>
            services.AddSingleton<ISuspensionDriver, AkavacheSuspensionDriver<AppState>>();
    }
}
