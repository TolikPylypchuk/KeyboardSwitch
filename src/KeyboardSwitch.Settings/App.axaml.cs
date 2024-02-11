namespace KeyboardSwitch.Settings;

using System.Reactive.Subjects;
using System.Reflection;

using Avalonia.Controls.ApplicationLifetimes;

using KeyboardSwitch.Core.Exceptions;
using KeyboardSwitch.Core.Logging;

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

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using Splat.Serilog;

public class App : Application, IEnableLogger
{
    private IClassicDesktopStyleApplicationLifetime desktop = null!;
    private Mutex? mutex;
    private ServiceProvider? serviceProvider;

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

        var openExternally = this.ConfigureSingleInstance(serviceProvider);
        this.ConfigureSuspensionDriver();

        this.Log().Info("Starting the settings app");

        GetRequiredService<IInitialSetupService>().InitializeKeyboardSwitchSetup();

        try
        {
            var appSettings = await GetRequiredService<IAppSettingsService>().GetAppSettings();
            var mainViewModel = new MainViewModel(appSettings);
            openExternally.InvokeCommand(mainViewModel.OpenExternally);

            return mainViewModel;
        } catch (IncompatibleAppVersionException e)
        {
            this.Log().Fatal(
                e,
                "Incompatible app version found in settings: {Version}. " +
                "Delete the settings and let the app recreate a compatible version",
                e.Version);

            this.desktop.Shutdown(2);
            return null!;
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configDirectory = GetConfigDirectory();
        var environment = PlatformDependent(windows: () => "windows", macos: () => "macos", linux: () => "linux");
        var genericProvider = this.JsonProvider(configDirectory, "appsettings.json");
        var platformSpecificProvider = this.JsonProvider(configDirectory, $"appsettings.{environment}.json");

        var config = new ConfigurationRoot(
            new List<IConfigurationProvider> { genericProvider, platformSpecificProvider });

        var settingsSection = config.GetSection("Settings");

        services
            .AddOptions()
            .AddLogging(logging => logging.AddSplat())
            .Configure<GlobalSettings>(settingsSection)
            .AddSingleton(Messages.ResourceManager)
            .AddSingleton<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher())
            .AddSuspensionDriver()
            .AddCoreKeyboardSwitchServices()
            .AddNativeKeyboardSwitchServices(config)
            .UseMicrosoftDependencyResolver();

        Locator.CurrentMutable.InitializeSplat();
        Locator.CurrentMutable.UseSerilogFullLogger(SerilogLoggerFactory.CreateLogger(config));

        Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Avalonia);
        Locator.CurrentMutable.RegisterConstant(RxApp.TaskpoolScheduler, TaskPoolKey);
        Locator.CurrentMutable.RegisterConstant<IBindingTypeConverter>(new ModifierMaskConverter());

        this.RegisterViews();

        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    }

    private void RegisterViews()
    {
        Locator.CurrentMutable.Register<IViewFor<MainViewModel>>(() => new MainWindow());

        Locator.CurrentMutable.Register<IViewFor<AboutViewModel>>(() => new AboutView());
        Locator.CurrentMutable.Register<IViewFor<CharMappingViewModel>>(() => new CharMappingView());
        Locator.CurrentMutable.Register<IViewFor<LayoutViewModel>>(() => new LayoutView());
        Locator.CurrentMutable.Register<IViewFor<MainContentViewModel>>(() => new MainContentView());
        Locator.CurrentMutable.Register<IViewFor<PreferencesViewModel>>(() => new PreferencesView());
        Locator.CurrentMutable.Register<IViewFor<ServiceViewModel>>(() => new ServiceView());
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
            window.Width = state.WindowWidth;
            window.Height = state.WindowHeight;
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
        }

        state.IsInitialized = true;
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        this.Log().Info("Shutting down the settings app");

        try
        {
            this.serviceProvider?.Dispose();
        } finally
        {
            this.mutex?.ReleaseMutex();
            this.mutex?.Dispose();
        }
    }

    private Subject<Unit> ConfigureSingleInstance(IServiceProvider services)
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName()?.Name ?? String.Empty;

        this.mutex = services
            .GetRequiredService<ISingleInstanceService>()
            .TryAcquireMutex(assemblyName);

        var namedPipeService = services.GetRequiredService<INamedPipeService>();

        namedPipeService.StartServer(assemblyName);

        var openExternally = new Subject<Unit>();

        namedPipeService.ReceivedString
            .Discard()
            .Subscribe(openExternally);

        return openExternally;
    }
}
