using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

using SharpHook.Providers;

namespace KeyboardSwitch.Core;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection AddCoreKeyboardSwitchServices(this IServiceCollection services) =>
        services
            .AddSingleton<IGlobalHookProvider>(UioHookProvider.Instance)
            .AddSingleton<IAccessibilityProvider>(UioHookProvider.Instance)
            .AddSingleton<IReactiveGlobalHook>(sp => new SimpleReactiveGlobalHook(
                GlobalHookType.Keyboard, sp.GetService<IScheduler>(), sp.GetService<IGlobalHookProvider>()))
            .AddSingleton<IEventSimulator, EventSimulator>()
            .AddSingleton<IKeyboardHookService, SharpHookService>()
            .AddSingleton<IAppSettingsService, JsonSettingsService>()
            .AddSingleton<ISwitchService, SwitchService>()
            .AddSingleton<INamedPipeService, NamedPipeService>()
            .AddSingleton<ISingleInstanceService, SingleInstanceService>()
            .AddSingleton<IFileSystem, FileSystem>();
}
