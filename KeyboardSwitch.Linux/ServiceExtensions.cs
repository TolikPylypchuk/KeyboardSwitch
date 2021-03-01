using KeyboardSwitch.Common;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Linux
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchLinuxServices(this IServiceCollection services) =>
            services
                .AddSingleton(BlobCacheFactory.CreateBlobCache);
    }
}
