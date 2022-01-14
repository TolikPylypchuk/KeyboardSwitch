namespace KeyboardSwitch.Core;

using TextCopy;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreKeyboardSwitchServices(this IServiceCollection services) =>
        services
            .AddSingleton<IReactiveGlobalHook, BlockingReactiveGlobalHook>()
            .AddSingleton<IKeyboardHookService, SharpHookService>()
            .AddSingleton<ITextService, ClipboardTextService>()
            .AddSingleton<IEventSimulator, EventSimulator>()
            .AddSingleton<IUserActivitySimulator, UserActivitySimulator>()
            .AddClipboard()
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
                    name))
            .AddSingleton<IScheduler>(Scheduler.Default);

    public static IServiceCollection AddClipboard(this IServiceCollection services)
    {
        services.InjectClipboard();
        return services;
    }
}
