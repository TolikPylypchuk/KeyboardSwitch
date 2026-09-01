using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

using SharpHook.Providers;

namespace KeyboardSwitch.Core;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCoreKeyboardSwitchServices() =>
            services
                .AddSingleton<IGlobalHookProvider>(UioHookProvider.Instance)
                .AddSingleton<IEventSimulationProvider>(UioHookProvider.Instance)
                .AddSingleton<IAccessibilityProvider>(UioHookProvider.Instance)
                .AddSingleton<IReactiveGlobalHook, ReactiveGlobalHook>()
                .AddSingleton<IEventSimulator>(sp => EventSimulator.Create(
                    ApplicationName, sp.GetRequiredService<IEventSimulationProvider>()))
                .AddSingleton<IKeyboardHookService, SharpHookService>()
                .AddSingleton<IUserActivitySimulator, SharpUserActivitySimulator>()
                .AddSingleton<IAppSettingsService, JsonSettingsService>()
                .AddSingleton<ISwitchService, SwitchService>()
                .AddSingleton<INamedPipeService, NamedPipeService>()
                .AddSingleton<ISingleInstanceService, SingleInstanceService>()
                .AddSingleton<IFileSystem, FileSystem>();
    }
}
