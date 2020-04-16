using GregsStack.InputSimulatorStandard;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Windows.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KeyboardSwitch.Common.Windows
{
    public static class SerivceExtensions
    {
        public static IServiceCollection AddKeyboardSwitchServices(this IServiceCollection services)
            => services
                .AddSingleton<IInputSimulator>(new InputSimulator())
                .AddSingleton<IModiferKeysService, ModifierKeysService>()
                .AddSingleton<IKeyboardHookService, KeyboardHookService>()
                .AddSingleton<ITextService, ClipboardTextService>()
                .AddSingleton<ISwitchService, SwitchService>();
    }
}
