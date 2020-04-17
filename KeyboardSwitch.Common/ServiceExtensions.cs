using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Common
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchServices(this IServiceCollection services)
            => services.AddSingleton<ISettingsService, BlobCacheSettingsService>();
    }
}
