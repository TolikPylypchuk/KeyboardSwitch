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

        private IServiceCollection AddConverters() =>
            services
                .AddSingleton<IBindingTypeConverter>(new AppThemeFromConverter())
                .AddSingleton<IBindingTypeConverter>(new AppThemeToConverter())
                .AddSingleton<IBindingTypeConverter>(new AppThemeVariantFromConverter())
                .AddSingleton<IBindingTypeConverter>(new AppThemeVariantToConverter())
                .AddSingleton<IBindingTypeConverter>(new EventMaskFromConverter())
                .AddSingleton<IBindingTypeConverter>(new EventMaskToConverter());
    }

    private static JsonConfigurationProvider JsonProvider(string directory, string fileName) =>
        new(new JsonConfigurationSource
        {
            Path = fileName,
            FileProvider = new PhysicalFileProvider(directory),
            Optional = true
        });
}
