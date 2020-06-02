using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using Akavache;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace KeyboardSwitch
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BlobCache.ApplicationName = nameof(KeyboardSwitch);

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .UseConsoleLifetime()
                .Build();

            var mutex = ConfigureSingleInstance(host.Services);

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));

            SubscribeToExternalCommands(host, logger);

            logger.LogInformation("KeyboardSwitch service execution started");

            try
            {
                host.Run();
            } catch (OperationCanceledException)
            {
                logger.LogInformation("KeyboardSwitch service execution was cancelled");
            } catch (Exception e)
            {
                logger.LogError(e, "Unknown error");
            } finally
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }

            logger.LogInformation("KeyboardSwitch service execution stopped");
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddHostedService<Worker>()
                .Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromMilliseconds(100))
                .Configure<GlobalSettings>(hostContext.Configuration.GetSection("Settings"))
                .AddSingleton<IScheduler>(Scheduler.Default)
                .AddKeyboardSwitchServices();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddKeyboardSwitchWindowsServices();
            }
        }

        private static void ConfigureLogging(HostBuilderContext hostingContext, ILoggingBuilder logging)
            => logging
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
                .Do(_ => logger.LogInformation("Invalidating the settings be external request"))
                .Subscribe(_ => settingsService.InvalidateAppSettings());

            namedPipeService.ReceivedString
                .Where(command => command.IsUnknownCommand())
                .Subscribe(command => logger.LogWarning($"External request '{command}' is not recognized"));
        }
    }
}
