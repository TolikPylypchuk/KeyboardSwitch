using KeyboardSwitch.Settings.Core.State;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSuspensionDriver(this IServiceCollection services) =>
            services.AddSingleton<ISuspensionDriver, AkavacheSuspensionDriver<AppState>>();
    }
}
