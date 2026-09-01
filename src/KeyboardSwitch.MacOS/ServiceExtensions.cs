using Microsoft.Extensions.Configuration;

namespace KeyboardSwitch.MacOS;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddNativeKeyboardSwitchServices(IConfiguration config) =>
            services
                .Configure<LaunchdSettings>(config.GetSection("Launchd"))
                .AddSingleton(SimulationModifierKeyCodeProvider.Command)
                .AddSingleton<ILayoutService, MacLayoutService>()
                .AddSingleton<IClipboardService, MacClipboardService>()
                .AddSingleton<IStartupService, LaunchdStartupService>()
                .AddSingleton<IServiceCommunicator, LaunchdServiceCommunicator>()
                .AddSingleton<IAutoConfigurationService, MacAutoConfigurationService>()
                .AddSingleton<IInitialSetupService, LaunchdSetupService>()
                .AddSingleton<IUserProvider, PosixUserProvider>()
                .AddSingleton<IMainLoopRunner, MacMainLoopRunner>();
    }
}
