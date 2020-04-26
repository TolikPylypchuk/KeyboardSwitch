using System;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;
using KeyboardSwitch.Common.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ReactiveUI;

using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Settings
{
    public static class Program
    {
        public static int Main(string[] args)
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .AddServices()
                .LogToDebug()
                .UseReactiveUI()
                .StartWithClassicDesktopLifetime(args);

        private static AppBuilder AddServices(this AppBuilder builder)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            var mutex = ConfigureSingleInstance(serviceProvider);

            builder.AfterSetup(newBuilder =>
            {
                if (newBuilder.Instance is App app)
                {
                    app.OnAppExitDisposable = Disposable.Create(() =>
                    {
                        serviceProvider.Dispose();
                        mutex.ReleaseMutex();
                        mutex.Dispose();
                    });
                }
            });

            return builder;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(ConfigureLogging)
                .AddKeyboardSwitchServices();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddKeyboardSwitchWindowsServices();
            }

            services.UseMicrosoftDependencyResolver();
        }

        private static void ConfigureLogging(ILoggingBuilder logging)
            => logging.AddDebug()
                .AddSplat();

        private static Mutex ConfigureSingleInstance(IServiceProvider services)
        {
            string assemblyName = Assembly.GetExecutingAssembly().FullName ?? String.Empty;

            var singleInstanceResolver = services.GetRequiredService<ServiceResolver<ISingleInstanceService>>();
            var singleInstanceService = singleInstanceResolver(assemblyName);

            var mutex = singleInstanceService.TryAcquireMutex();

            var namedPipeResolver = services.GetRequiredService<ServiceResolver<INamedPipeService>>();
            var namedPipeService = namedPipeResolver(assemblyName);

            namedPipeService.StartServer();

            return mutex;
        }
    }
}
