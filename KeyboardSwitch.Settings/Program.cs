using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using Akavache;

using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Common.Windows;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

using ReactiveUI;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Settings
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI()
                .Configure(services)
                .StartWithClassicDesktopLifetime(args);
        }

        private static AppBuilder Configure(this AppBuilder builder, IServiceCollection services)
            => builder.AfterSetup(newBuilder =>
            {
                if (newBuilder.Instance is App app)
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.UseMicrosoftDependencyResolver();

                    var mutex = ConfigureSingleInstance(serviceProvider);

                    app.OnAppExitDisposable = Disposable.Create(() =>
                    {
                        serviceProvider.DisposeAsync().AsTask().Wait();
                        mutex.ReleaseMutex();
                        mutex.Dispose();
                    });
                }
            });

        private static void ConfigureServices(IServiceCollection services)
        {
            var provider = new JsonConfigurationProvider(new JsonConfigurationSource
            {
                Path = "appsettings.json",
                FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory)
            });

            var config = new ConfigurationRoot(new List<IConfigurationProvider> { provider });

            services
                .AddLogging(logging => ConfigureLogging(config, logging))
                .Configure<GlobalSettings>(config.GetSection("Settings"))
                .AddKeyboardSwitchServices();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddKeyboardSwitchWindowsServices();
            }

            services.UseMicrosoftDependencyResolver();

            Locator.CurrentMutable.InitializeSplat();
            Locator.CurrentMutable.InitializeReactiveUI();
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            BlobCache.ApplicationName = nameof(KeyboardSwitch);
        }

        private static void ConfigureLogging(IConfiguration config, ILoggingBuilder logging)
            => logging
                .AddConfiguration(config.GetSection("Logging"))
                .AddDebug()
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
