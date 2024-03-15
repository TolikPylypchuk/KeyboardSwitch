namespace KeyboardSwitch.Core;

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection AddCoreKeyboardSwitchServices(this IServiceCollection services) =>
        services
            .AddSingleton<IReactiveGlobalHook>(sp => new SimpleReactiveGlobalHook(GlobalHookType.Keyboard))
            .AddSingleton<IEventSimulator, EventSimulator>()
            .AddSingleton<IKeyboardHookService, SharpHookService>()
            .AddSingleton<ITextService, ClipboardTextService>()
            .AddSingleton<IAppSettingsService, JsonSettingsService>()
            .AddSingleton<ISwitchService, SwitchService>()
            .AddSingleton<INamedPipeService, NamedPipeService>()
            .AddSingleton<ISingleInstanceService, SingleInstanceService>()
            .AddSingleton<IFileSystem, FileSystem>();
}
