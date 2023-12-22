namespace KeyboardSwitch.Core;

using TextCopy;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreKeyboardSwitchServices(this IServiceCollection services) =>
        services
            .AddClipboard()
            .AddSingleton<IReactiveGlobalHook, SimpleReactiveGlobalHook>()
            .AddSingleton<IEventSimulator, EventSimulator>()
            .AddSingleton<IKeyboardHookService, SharpHookService>()
            .AddSingleton<ITextService, ClipboardTextService>()
            .AddSingleton(BlobCacheFactory.CreateBlobCache)
            .AddSingleton<BlobCacheSettingsService>()
            .AddSingleton<IAppSettingsService, BlobCacheSettingsService>()
            .AddSingleton<ISwitchService, SwitchService>()
            .AddSingleton<INamedPipeService, NamedPipeService>()
            .AddSingleton<ISingleInstanceService, SingleInstanceService>();

    public static IServiceCollection AddClipboard(this IServiceCollection services)
    {
        services.InjectClipboard();
        return services;
    }
}
