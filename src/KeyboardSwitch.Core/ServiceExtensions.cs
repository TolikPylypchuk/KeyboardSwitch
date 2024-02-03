namespace KeyboardSwitch.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreKeyboardSwitchServices(this IServiceCollection services) =>
        services
            .AddSingleton<IReactiveGlobalHook, SimpleReactiveGlobalHook>()
            .AddSingleton<IEventSimulator, EventSimulator>()
            .AddSingleton<IKeyboardHookService, SharpHookService>()
            .AddSingleton<ITextService, ClipboardTextService>()
            .AddSingleton<IAppSettingsService, JsonSettingsService>()
            .AddSingleton<ISwitchService, SwitchService>()
            .AddSingleton<INamedPipeService, NamedPipeService>()
            .AddSingleton<ISingleInstanceService, SingleInstanceService>();
}
