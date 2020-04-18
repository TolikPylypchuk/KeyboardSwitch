using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
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

            logger.LogInformation("KeyboardSwitch service execution started");

            try
            {
                await host.RunAsync();
            } catch (OperationCanceledException)
            {
                logger.LogInformation("KeyboardSwitch service execution is cancelled");
            } catch (Exception e)
            {
                logger.LogError(e, "Unknown error");
            }

            logger.LogInformation("KeyboardSwitch service execution stopped");
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddHostedService<Worker>()
                .Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(1))
                .Configure<GlobalSettings>(hostContext.Configuration.GetSection("Settings"))
                .AddKeyboardSwitchServices();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddKeyboardSwitchWindowsServices();
            }
        }
    }
}
