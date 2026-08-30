using Avalonia.Controls.Templates;

using KeyboardSwitch.Core.Logging;

#if WINDOWS
using KeyboardSwitch.Windows;
#elif MACOS
using KeyboardSwitch.MacOS;
#elif LINUX
using KeyboardSwitch.Linux;
#endif

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using Serilog;

using Splat;

namespace KeyboardSwitch.Settings;

public static class ServiceExtensions
{
    private static JsonConfigurationProvider JsonProvider(string directory, string fileName) =>
        new(new JsonConfigurationSource
        {
            Path = fileName,
            FileProvider = new PhysicalFileProvider(directory),
            Optional = true
        });

    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureServices()
        {
            var configDirectory = GetConfigDirectory();
            var environment = PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux");

            var genericProvider = JsonProvider(configDirectory, "appsettings.json");
            var platformSpecificProvider = JsonProvider(configDirectory, $"appsettings.{environment}.json");

            var config = new ConfigurationRoot([genericProvider, platformSpecificProvider]);

            return services
                .AddOptions()
                .AddLogging(config)
                .Configure<GlobalSettings>(config.GetSection("Settings"))
                .AddCoreKeyboardSwitchServices()
                .AddNativeKeyboardSwitchServices(config)
                .AddViews()
                .AddConverters()
                .AddSingleton(Messages.ResourceManager)
                .AddSingleton<ISuspensionDriver, JsonSuspensionDriver>();
        }

        private IServiceCollection AddLogging(IConfiguration config)
        {
            var logger = SerilogLoggerFactory.CreateLogger(config, addLibUioHookLogging: false);
            Log.Logger = logger;

            return services
                .AddLogging(config => config.AddSerilog(logger))
                .AddSingleton<ILogManager>(sp =>
                    new FuncLogManager(type => new SerilogFullLogger(logger.ForContext(type))));
        }

        private IServiceCollection AddViews() =>
            services
                .AddSingleton<IDataTemplate, ViewLocator>()
                .AddSingleton<IViewFor<MainViewModel>>(sp => new MainWindow())
                .AddTransient<IViewFor<AboutViewModel>>(sp => new AboutView())
                .AddTransient<IViewFor<CharMappingViewModel>>(sp => new CharMappingView())
                .AddTransient<IViewFor<LayoutViewModel>>(sp => new LayoutView())
                .AddTransient<IViewFor<MainContentViewModel>>(sp => new MainContentView())
                .AddTransient<IViewFor<PreferencesViewModel>>(sp => new PreferencesView())
                .AddTransient<IViewFor<ServiceViewModel>>(sp => new ServiceView());

        private IServiceCollection AddConverters() =>
            services
                .AddSingleton<IBindingTypeConverter>(new AppThemeFromConverter())
                .AddSingleton<IBindingTypeConverter>(new AppThemeToConverter())
                .AddSingleton<IBindingTypeConverter>(new AppThemeVariantFromConverter())
                .AddSingleton<IBindingTypeConverter>(new AppThemeVariantToConverter())
                .AddSingleton<IBindingTypeConverter>(new EventMaskFromConverter())
                .AddSingleton<IBindingTypeConverter>(new EventMaskToConverter());
    }
}
