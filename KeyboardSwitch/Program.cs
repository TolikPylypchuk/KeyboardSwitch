using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using Akavache;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Common.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));

            var mutex = ConfigureSingleInstance(host, logger);

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
                .AddKeyboardSwitchServices();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddKeyboardSwitchWindowsServices();
            }
        }

        private static void ConfigureLogging(HostBuilderContext hostingContext, ILoggingBuilder logging)
            => logging
                .ClearProviders()
                .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                .AddConsole()
                .AddDebug();

        private static Mutex ConfigureSingleInstance(IHost host, ILogger logger)
        {
            var singleInstanceResolver = host.Services.GetRequiredService<ServiceResolver<ISingleInstanceService>>();
            var singleInstanceService = singleInstanceResolver(nameof(KeyboardSwitch));

            var mutex = singleInstanceService.TryAcquireMutex();

            var namedPipeResolver = host.Services.GetRequiredService<ServiceResolver<INamedPipeService>>();
            var namedPipeService = namedPipeResolver(nameof(KeyboardSwitch));

            namedPipeService.StartServer();

            namedPipeService.ReceivedString
                .Where(command => command.IsCommand(ExternalCommand.Stop))
                .Do(_ => logger.LogInformation("Stopping the service by external request"))
                .SubscribeAsync(async _ => await host.StopAsync());

            return mutex;
        }
    }
}
