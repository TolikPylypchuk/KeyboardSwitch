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
using KeyboardSwitch.Settings.Core;
using KeyboardSwitch.Settings.Properties;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using ReactiveUI;

using Serilog;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using Splat.Serilog;

using static KeyboardSwitch.Settings.Core.Constants;

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

        private static void ConfigureServices(IServiceCollection services)
        {
            var provider = new JsonConfigurationProvider(new JsonConfigurationSource
            {
                Path = "appsettings.json",
                FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory)
            });

            var config = new ConfigurationRoot(new List<IConfigurationProvider> { provider });

            services
                .AddLogging(logging => logging.AddSplat())
                .Configure<GlobalSettings>(config.GetSection("Settings"))
                .AddSingleton(Messages.ResourceManager)
                .AddSuspensionDriver()
                .AddKeyboardSwitchServices();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddKeyboardSwitchWindowsServices();
            }

            services.UseMicrosoftDependencyResolver();

            BlobCache.ApplicationName = nameof(KeyboardSwitch);

            Locator.CurrentMutable.InitializeSplat();
            Locator.CurrentMutable.UseSerilogFullLogger(
                new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .Enrich.FromLogContext()
                    .CreateLogger());

            Locator.CurrentMutable.InitializeReactiveUI();
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            Locator.CurrentMutable.RegisterConstant(RxApp.TaskpoolScheduler, TaskPoolKey);
        }

        private static AppBuilder Configure(this AppBuilder builder, IServiceCollection services)
            => builder.AfterSetup(newBuilder =>
            {
                if (newBuilder.Instance is App app)
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.UseMicrosoftDependencyResolver();

                    var mutex = ConfigureSingleInstance(serviceProvider, app);

                    app.OnAppExitDisposable = Disposable.Create(() =>
                    {
                        serviceProvider.DisposeAsync().AsTask().Wait();
                        mutex.ReleaseMutex();
                        mutex.Dispose();
                    });
                }
            });

        private static Mutex ConfigureSingleInstance(IServiceProvider services, App app)
        {
            string assemblyName = Assembly.GetExecutingAssembly().FullName ?? String.Empty;

            var singleInstanceProvider = services.GetRequiredService<ServiceProvider<ISingleInstanceService>>();
            var singleInstanceService = singleInstanceProvider(assemblyName);

            var mutex = singleInstanceService.TryAcquireMutex();

            var namedPipeProvider = services.GetRequiredService<ServiceProvider<INamedPipeService>>();
            var namedPipeService = namedPipeProvider(assemblyName);

            namedPipeService.StartServer();

            namedPipeService.ReceivedString
                .Discard()
                .Subscribe(app.OpenExternally);

            return mutex;
        }
    }
}
