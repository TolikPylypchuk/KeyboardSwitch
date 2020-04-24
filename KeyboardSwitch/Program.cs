using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .Build();

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));

            ConfigureSingleInstance(host, logger);

            logger.LogInformation("KeyboardSwitch service execution started");

            try
            {
                await host.RunAsync();
            } catch (OperationCanceledException)
            {
                logger.LogInformation("KeyboardSwitch service execution was cancelled");
            } catch (Exception e)
            {
                logger.LogError(e, "Unknown error");
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

        private static void ConfigureSingleInstance(IHost host, ILogger logger)
        {
            var singleInstanceResolver = host.Services.GetRequiredService<ServiceResolver<ISingleInstanceService>>();
            var singleInstanceService = singleInstanceResolver(nameof(KeyboardSwitch));

            var mutex = singleInstanceService.TryAcquireMutex();

            GC.KeepAlive(mutex);

            var namedPipeResolver = host.Services.GetRequiredService<ServiceResolver<INamedPipeService>>();
            var namedPipeService = namedPipeResolver(nameof(KeyboardSwitch));

            namedPipeService.StartServer();

            namedPipeService.ReceivedString
                .Where(command => command.IsCommand(ExternalCommand.Stop))
                .Do(_ => logger.LogInformation("Stopping the service by external request"))
                .SubscribeAsync(async _ => await host.StopAsync());
        }
    }
}
