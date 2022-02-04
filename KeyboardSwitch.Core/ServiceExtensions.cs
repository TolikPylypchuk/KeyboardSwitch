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
            .AddSingleton<IAppSettingsService>(provider =>
                provider.GetRequiredService<BlobCacheSettingsService>())
            .AddSingleton<IConverterSettingsService>(provider =>
                provider.GetRequiredService<BlobCacheSettingsService>())
            .AddSingleton<ISwitchService, SwitchService>()
            .AddSingleton<ServiceProvider<INamedPipeService>>(s => name =>
                new NamedPipeService(s.GetRequiredService<ILogger<NamedPipeService>>(), name))
            .AddSingleton<ServiceProvider<ISingleInstanceService>>(s => name =>
                new SingleInstanceService(
                    s.GetRequiredService<ServiceProvider<INamedPipeService>>(),
                    s.GetRequiredService<ILogger<SingleInstanceService>>(),
                    name));

    public static IServiceCollection AddClipboard(this IServiceCollection services)
    {
        services.InjectClipboard();
        return services;
    }
}
