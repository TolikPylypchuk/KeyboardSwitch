namespace KeyboardSwitch.Settings;

using System.Reactive.Subjects;
using System.Reflection;

using Akavache;

using Avalonia.Controls.ApplicationLifetimes;

#if WINDOWS
using KeyboardSwitch.Windows;
#elif MACOS
using KeyboardSwitch.MacOS;
#elif LINUX
using KeyboardSwitch.Linux;
#endif

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using Serilog;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using Splat.Serilog;

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

            var mainViewModel = await this.InitializeApp();

            this.desktop.MainWindow = await this.CreateMainWindow(mainViewModel);
            this.desktop.MainWindow.Show();

            GetDefaultService<IInitialSetupService>().InitializeKeyboardSwitchSetup();

            this.desktop.Exit += this.OnExit;
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
            var appSettings = await GetDefaultService<IAppSettingsService>().GetAppSettings();

            var mainViewModel = new MainViewModel(appSettings);
            this.openExternally.InvokeCommand(mainViewModel.OpenExternally);

            return mainViewModel;
        } catch (IncompatibleAppVersionException e)
        {
            var settingsPath = Environment.ExpandEnvironmentVariables(
                GetDefaultService<IOptions<GlobalSettings>>().Value.SettingsFilePath);

            this.Log().Error(
                e,
                "Incompatible app version found in settings: {Version}. " +
                "Delete the settings at '{SettingsPath}' and let the app recreate a compatible version",
                e.Version,
                settingsPath);

            this.desktop.Shutdown(1);
            return null!;
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configDir = GetConfigDirectory();
        var environment = PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux");
        var genericProvider = this.JsonProvider(configDir, "appsettings.json");
        var platformSpecificProvider = this.JsonProvider(configDir, $"appsettings.{environment}.json");

        var config = new ConfigurationRoot(
            new List<IConfigurationProvider> { genericProvider, platformSpecificProvider });

        services
            .AddLogging(logging => logging.AddSplat())
            .Configure<GlobalSettings>(config.GetSection("Settings"))
            .AddSingleton(Messages.ResourceManager)
            .AddSingleton<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher())
            .AddSuspensionDriver()
            .AddCoreKeyboardSwitchServices()
#if WINDOWS || MACOS || LINUX
            .AddNativeKeyboardSwitchServices(config)
#endif
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
        Locator.CurrentMutable.RegisterConstant<IBindingTypeConverter>(new ModifierMaskConverter());

        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    }

    private void ConfigureSuspensionDriver()
    {
        var autoSuspendHelper = new AutoSuspendHelper(this.desktop);

        RxApp.SuspensionHost.CreateNewAppState = () => new AppState();
        RxApp.SuspensionHost.SetupDefaultSuspendResume();

        autoSuspendHelper.OnFrameworkInitializationCompleted();
    }

    private JsonConfigurationProvider JsonProvider(string directory, string fileName) =>
        new(new JsonConfigurationSource
        {
            Path = fileName,
            FileProvider = new PhysicalFileProvider(directory),
            Optional = true
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

        var windowStateChanged = window
            .GetObservable(Window.WindowStateProperty)
            .DistinctUntilChanged()
            .Discard();

        var windowResized = window
            .GetObservable(TopLevel.ClientSizeProperty)
            .DistinctUntilChanged()
            .Discard();

        var windowPositionChanged = Observable
            .FromEventPattern<PixelPointEventArgs>(h => window.PositionChanged += h, h => window.PositionChanged -= h)
            .Discard();

        Observable.Merge(windowStateChanged, windowResized, windowPositionChanged)
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
        string assemblyName = Assembly.GetExecutingAssembly().GetName()?.Name ?? String.Empty;

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