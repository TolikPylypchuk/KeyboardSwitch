using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;
using KeyboardSwitch.Common.Settings;

#if WINDOWS
using KeyboardSwitch.Windows;
#else
using KeyboardSwitch.Linux;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using static KeyboardSwitch.Common.Util;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace KeyboardSwitch
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? String.Empty);

            BlobCache.ApplicationName = nameof(KeyboardSwitch);

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .UseConsoleLifetime()
                .UseEnvironment(PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux"))
                .Build();

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));

            using var mutex = ConfigureSingleInstance(host.Services);

            try
            {
                SubscribeToExternalCommands(host, logger);

                logger.LogInformation("KeyboardSwitch service execution started");

                host.Run();

                logger.LogInformation("KeyboardSwitch service execution stopped");
            } catch (Exception e) when (e is OperationCanceledException || e is TaskCanceledException)
            {
                logger.LogInformation("The Keyboard Switch service execution was cancelled");
            } finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services) =>
            services.AddHostedService<Worker>()
                .Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
                .Configure<GlobalSettings>(hostContext.Configuration.GetSection("Settings"))
                .AddSingleton<IScheduler>(Scheduler.Default)
                .AddKeyboardSwitchServices()
#if WINDOWS
                .AddKeyboardSwitchWindowsServices();
#else
                .AddKeyboardSwitchLinuxServices();
#endif

        private static void ConfigureLogging(HostBuilderContext hostingContext, ILoggingBuilder logging) =>
            logging
                .ClearProviders()
                .AddSerilog(
                    new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .CreateLogger(),
                    dispose: true);

        private static Mutex ConfigureSingleInstance(IServiceProvider services)
        {
            var singleInstanceProvider = services.GetRequiredService<ServiceProvider<ISingleInstanceService>>();
            var singleInstanceService = singleInstanceProvider(nameof(KeyboardSwitch));

            return singleInstanceService.TryAcquireMutex();
        }

        private static void SubscribeToExternalCommands(IHost host, ILogger logger)
        {
            var namedPipeProvider = host.Services.GetRequiredService<ServiceProvider<INamedPipeService>>();
            var namedPipeService = namedPipeProvider(nameof(KeyboardSwitch));

            var settingsService = host.Services.GetRequiredService<IAppSettingsService>();

            namedPipeService.StartServer();

            namedPipeService.ReceivedString
                .Where(command => command.IsCommand(ExternalCommand.Stop))
                .Do(_ => logger.LogInformation("Stopping the service by external request"))
                .SubscribeAsync(async _ => await host.StopAsync());

            namedPipeService.ReceivedString
                .Where(command => command.IsCommand(ExternalCommand.ReloadSettings))
                .Do(_ => logger.LogInformation("Invalidating the settings by external request"))
                .Subscribe(_ => settingsService.InvalidateAppSettings());

            namedPipeService.ReceivedString
                .Where(command => command.IsUnknownCommand())
                .Subscribe(command => logger.LogWarning($"External request '{command}' is not recognized"));
        }
    }
}
