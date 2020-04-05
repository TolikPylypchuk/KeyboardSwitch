using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Windows.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Common.Windows
{
    public static class SerivceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchServices(this IServiceCollection services)
            => services
                .AddSingleton<IModiferKeysService, ModifierKeysService>()
                .AddSingleton<IKeyboardHookService, KeyboardHookService>();
    }
}
