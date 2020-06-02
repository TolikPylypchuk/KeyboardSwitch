using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Akavache;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core;
using KeyboardSwitch.Settings.Core.State;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Infrastructure;
using KeyboardSwitch.Settings.Properties;
using KeyboardSwitch.Settings.Views;
using KeyboardSwitch.Windows;

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
    public class App : Application, IEnableLogger
    {
        private IClassicDesktopStyleApplicationLifetime desktop = null!;
        private Mutex? mutex;
        private ServiceProvider? serviceProvider;

        private readonly Subject<Unit> openExternally = new Subject<Unit>();

        public override void Initialize()
            => AvaloniaXamlLoader.Load(this);

        public override async void OnFrameworkInitializationCompleted()
        {
            this.Log().Info("Starting the settings app");

            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                this.desktop = desktop;

                var services = new ServiceCollection();

                this.ConfigureServices(services);

                this.serviceProvider = services.BuildServiceProvider();
                this.serviceProvider.UseMicrosoftDependencyResolver();

                this.ConfigureSingleInstance(serviceProvider);
                this.ConfigureSuspensionDriver();

                var appSettings = await Locator.Current.GetService<IAppSettingsService>().GetAppSettingsAsync();
                var converterSettings = await Locator.Current.GetService<IConverterSettingsService>()
                    .GetConverterSettingsAsync();

                var mainViewModel = new MainViewModel(appSettings, converterSettings);

                this.openExternally.InvokeCommand(mainViewModel.OpenExternally);

                desktop.MainWindow = await this.CreateMainWindow(mainViewModel);
                desktop.MainWindow.Show();

                desktop.Exit += this.OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
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
                .AddSingleton<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher())
                .AddSingleton<IPropertyBindingHook>(new BindingHook())
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

            Locator.CurrentMutable.RegisterConstant(new ModifierKeyConverter(), typeof(IBindingTypeConverter));
            Locator.CurrentMutable.RegisterConstant(new SwitchModeConverter(), typeof(IBindingTypeConverter));

            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        }

        private void ConfigureSuspensionDriver()
        {
            var autoSuspendHelper = new AutoSuspendHelper(this.desktop);
            GC.KeepAlive(autoSuspendHelper);

            RxApp.SuspensionHost.CreateNewAppState = () => new AppState();
            RxApp.SuspensionHost.SetupDefaultSuspendResume();

            autoSuspendHelper.OnFrameworkInitializationCompleted();
        }

        private async Task<MainWindow> CreateMainWindow(MainViewModel viewModel)
        {
            var state = await RxApp.SuspensionHost.ObserveAppState<AppState>().Take(1);

            var window = new MainWindow
            {
                ViewModel = viewModel
            };

            if (state.IsInitialized)
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Width = state.WindowWidth;
                window.Height = state.WindowHeight;
                window.Position = new PixelPoint(state.WindowX, state.WindowY);
                window.WindowState = state.IsWindowMaximized ? WindowState.Maximized : WindowState.Normal;
            }

            window.GetObservable(Window.WindowStateProperty)
                .DistinctUntilChanged()
                .Discard()
                .Merge(window.GetObservable(TopLevel.ClientSizeProperty).DistinctUntilChanged().Discard())
                .Merge(Observable.FromEventPattern<EventArgs>(window, nameof(window.PositionChanged)).Discard())
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.SaveAppState);

            return window;
        }

        private void SaveAppState()
        {
            if (this.desktop.MainWindow == null)
            {
                return;
            }

            var state = RxApp.SuspensionHost.GetAppState<AppState>();

            state.WindowWidth = this.desktop.MainWindow.Width;
            state.WindowHeight = this.desktop.MainWindow.Height;
            state.WindowX = this.desktop.MainWindow.Position.X;
            state.WindowY = this.desktop.MainWindow.Position.Y;
            state.IsWindowMaximized = this.desktop.MainWindow.WindowState == WindowState.Maximized;
            state.IsInitialized = true;
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            this.Log().Info("Shutting down the settings app");

            this.serviceProvider?.DisposeAsync().AsTask().Wait();
            this.mutex?.ReleaseMutex();
            this.mutex?.Dispose();
        }

        private void ConfigureSingleInstance(IServiceProvider services)
        {
            string assemblyName = Assembly.GetExecutingAssembly().FullName ?? String.Empty;

            var singleInstanceProvider = services.GetRequiredService<ServiceProvider<ISingleInstanceService>>();
            var singleInstanceService = singleInstanceProvider(assemblyName);

            this.mutex = singleInstanceService.TryAcquireMutex();

            var namedPipeProvider = services.GetRequiredService<ServiceProvider<INamedPipeService>>();
            var namedPipeService = namedPipeProvider(assemblyName);

            namedPipeService.StartServer();

            namedPipeService.ReceivedString
                .Discard()
                .Subscribe(this.openExternally);
        }
    }
}
