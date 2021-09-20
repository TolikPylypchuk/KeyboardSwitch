using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Akavache;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

using KeyboardSwitch.Core;
using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Core.Services.Infrastructure;
using KeyboardSwitch.Core.Settings;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core;
using KeyboardSwitch.Settings.Core.State;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Properties;
using KeyboardSwitch.Settings.Views;

#if WINDOWS
using KeyboardSwitch.Windows;
#else
using KeyboardSwitch.Linux;
#endif

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using ReactiveUI;

using Serilog;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using Splat.Serilog;

using static KeyboardSwitch.Core.Util;
using static KeyboardSwitch.Settings.Core.Constants;
using static KeyboardSwitch.Settings.Core.ServiceUtil;

using KeyboardSwitch.Core.Services.Startup;
using KeyboardSwitch.Core.Services.Settings;
using FluentAvalonia.Styling;

namespace KeyboardSwitch.Settings
{
    public class App : Application, IEnableLogger
    {
        private const string SetStartupFile = "set-startup";

        private IClassicDesktopStyleApplicationLifetime desktop = null!;
        private Mutex? mutex;
        private ServiceProvider? serviceProvider;

        private readonly Subject<Unit> openExternally = new();

        public override void Initialize() =>
            AvaloniaXamlLoader.Load(this);

        public override async void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                this.desktop = desktop;
                this.desktop.Exit += this.OnExit;

                var mainViewModel = await this.InitializeApp();

                var fluentAvaloniaTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
                fluentAvaloniaTheme.RequestedTheme = "Light";

                this.desktop.MainWindow = await this.CreateMainWindow(mainViewModel);
                this.desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task<MainViewModel> InitializeApp()
        {
            TransitioningContentControl.PageTransitionProperty.OverrideDefaultValue(typeof(ViewModelViewHost), null);

            var services = new ServiceCollection();

            this.ConfigureServices(services);

            this.serviceProvider = services.BuildServiceProvider();
            this.serviceProvider.UseMicrosoftDependencyResolver();

            this.Log().Info("Starting the settings app");

            this.ConfigureSingleInstance(serviceProvider);
            this.ConfigureSuspensionDriver();

            try
            {
                var appSettings = await GetDefaultService<IAppSettingsService>().GetAppSettingsAsync();

                if (File.Exists(SetStartupFile))
                {
                    this.Log().Info("Setting the app to run at system startup");
                    GetDefaultService<IStartupService>().ConfigureStartup(appSettings, true);
                    File.Delete(SetStartupFile);
                }

                var converterSettings = await GetDefaultService<IConverterSettingsService>()
                    .GetConverterSettingsAsync();

                var mainViewModel = new MainViewModel(appSettings, converterSettings);

                this.openExternally.InvokeCommand(mainViewModel.OpenExternally);

                return mainViewModel;
            } catch (IncompatibleAppVersionException e)
            {
                var settingsPath = Environment.ExpandEnvironmentVariables(
                    GetDefaultService<IOptions<GlobalSettings>>().Value.Path);

                this.Log().Error(e, $"Incompatible app version found in settings: {e.Version}. " +
                    $"Delete the settings at '{settingsPath}' and let the app recreate a compatible version");

                this.desktop.Shutdown(1);
                return null!;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var environment = PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux");
            var genericProvider = this.JsonProvider("appsettings.json");
            var platformSpecificProvider = this.JsonProvider($"appsettings.{environment}.json");

            var config = new ConfigurationRoot(
                new List<IConfigurationProvider> { genericProvider, platformSpecificProvider });

            services
                .AddLogging(logging => logging.AddSplat())
                .Configure<GlobalSettings>(config.GetSection("Settings"))
                .AddSingleton(Messages.ResourceManager)
                .AddSingleton<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher())
                .AddSuspensionDriver()
                .AddCoreKeyboardSwitchServices()
                .AddNativeKeyboardSwitchServices()
                .UseMicrosoftDependencyResolver();

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
            Locator.CurrentMutable.RegisterConstant<IBindingTypeConverter>(new KeyCodeConverter());
            Locator.CurrentMutable.RegisterConstant<IBindingTypeConverter>(new ModifierKeyConverter());

            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        }

        private void ConfigureSuspensionDriver()
        {
            var autoSuspendHelper = new AutoSuspendHelper(this.desktop);

            RxApp.SuspensionHost.CreateNewAppState = () => new AppState();
            RxApp.SuspensionHost.SetupDefaultSuspendResume();

            autoSuspendHelper.OnFrameworkInitializationCompleted();
        }

        private IConfigurationProvider JsonProvider(string fileName) =>
            new JsonConfigurationProvider(new JsonConfigurationSource
            {
                Path = fileName,
                FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory)
            });

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

            state.IsWindowMaximized = this.desktop.MainWindow.WindowState == WindowState.Maximized;

            if (!state.IsWindowMaximized)
            {
                state.WindowWidth = this.desktop.MainWindow.Width;
                state.WindowHeight = this.desktop.MainWindow.Height;
                state.WindowX = this.desktop.MainWindow.Position.X;
                state.WindowY = this.desktop.MainWindow.Position.Y;
            }

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
