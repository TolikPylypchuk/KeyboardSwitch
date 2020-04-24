using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchServices(this IServiceCollection services)
            => services.AddSingleton<ISettingsService, BlobCacheSettingsService>()
                .AddSingleton<ServiceResolver<INamedPipeService>>(s => name =>
                    new NamedPipeService(s.GetRequiredService<ILogger<NamedPipeService>>(), name))
                .AddSingleton<ServiceResolver<ISingleInstanceService>>(s => name =>
                    new SingleInstanceService(
                        s.GetRequiredService<ServiceResolver<INamedPipeService>>(),
                        s.GetRequiredService<ILogger<SingleInstanceService>>(),
                        name));
    }
}
