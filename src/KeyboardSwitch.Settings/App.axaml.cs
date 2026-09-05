using System.Reactive.Subjects;
using System.Reflection;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Simple;

using FluentAvalonia.Styling;

using KeyboardSwitch.Core.Exceptions;
using KeyboardSwitch.Settings.Themes;

using Splat;

namespace KeyboardSwitch.Settings;

public class App : Application, IEnableLogger
{
    private Mutex? mutex;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = await this.InitializeApp(desktop);

            var mainWindow = this.CreateMainWindow(mainViewModel);
            this.SetWindowSizeFromState(mainWindow);

            desktop.MainWindow = mainWindow;
            desktop.MainWindow.Show();

            desktop.Exit += this.OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task<MainViewModel> InitializeApp(IClassicDesktopStyleApplicationLifetime desktop)
    {
        TransitioningContentControl.PageTransitionProperty.OverrideDefaultValue(typeof(ViewModelViewHost), null);

        var openExternally = this.ConfigureSingleInstance();
        this.ConfigureSuspensionDriver(desktop);

        this.Log().Info("Starting the settings app");

        AppLocator.Current.GetRequiredService<IInitialSetupService>().InitializeKeyboardSwitchSetup();

        try
        {
            var appSettings = await AppLocator.Current.GetRequiredService<IAppSettingsService>().GetAppSettings();
            var layouts = await AppLocator.Current.GetRequiredService<ILayoutService>().GetKeyboardLayouts();

            var mainViewModel = new MainViewModel(appSettings, layouts);
            openExternally.InvokeCommand(mainViewModel.OpenExternallyCommand);

            mainViewModel.PreferencesSaved
                .Select(p => p.AppTheme)
                .StartWith(appSettings.AppTheme)
                .DistinctUntilChanged()
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(theme => this.SetTheme(desktop, theme));

            mainViewModel.PreferencesSaved
                .Select(p => p.AppThemeVariant)
                .StartWith(appSettings.AppThemeVariant)
                .DistinctUntilChanged()
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(this.SetThemeVariant);

            return mainViewModel;
        } catch (IncompatibleAppVersionException e)
        {
            this.Log().Fatal(
                e,
                "Incompatible app version found in settings: {Version}. " +
                "Delete the settings and let the app recreate a compatible version",
                e.Version);

            desktop.Shutdown((int)ExitCode.IncompatibleSettingsVersion);
            return null!;
        }
    }

    private void ConfigureSuspensionDriver(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var autoSuspendHelper = new AutoSuspendHelper(desktop);

        RxSuspension.SuspensionHost.CreateNewAppState = () => new AppState();
        RxSuspension.SuspensionHost.SetupDefaultSuspendResume();

        autoSuspendHelper.OnFrameworkInitializationCompleted();
    }

    private MainWindow CreateMainWindow(MainViewModel viewModel)
    {
        var window = new MainWindow
        {
            ViewModel = viewModel
        };

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
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(() => this.SaveAppState(window));

        return window;
    }

    private void SetWindowSizeFromState(MainWindow window)
    {
        if (RxSuspension.SuspensionHost.AppState is AppState state && state.IsInitialized)
        {
            window.Width = state.WindowWidth;
            window.Height = state.WindowHeight;
            window.WindowState = state.IsWindowMaximized ? WindowState.Maximized : WindowState.Normal;
        }
    }

    private void SetTheme(IClassicDesktopStyleApplicationLifetime desktop, AppTheme appTheme)
    {
        this.Styles[0] = appTheme switch
        {
            AppTheme.MacOS => new MacOSTheme(),
            AppTheme.Simple => new SimpleTheme(),
            _ => new FluentAvaloniaTheme
            {
                PreferUserAccentColor = true,
                PreferSystemTheme = true
            }
        };

        if (desktop.MainWindow is MainWindow window)
        {
            var newMainWindow = this.CreateMainWindow(window.ViewModel!);

            newMainWindow.Width = window.Width;
            newMainWindow.Height = window.Height;
            newMainWindow.WindowState = window.WindowState;

            desktop.MainWindow = newMainWindow;
            desktop.MainWindow.Show();

            newMainWindow.Position = window.Position;

            window.Close();
        }
    }

    private void SetThemeVariant(AppThemeVariant appThemeVariant)
    {
        this.RequestedThemeVariant = appThemeVariant switch
        {
            AppThemeVariant.Light => ThemeVariant.Light,
            AppThemeVariant.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    private void SaveAppState(Window window)
    {
        var state = RxSuspension.SuspensionHost.GetAppState<AppState>();

        state.IsWindowMaximized = window.WindowState == WindowState.Maximized;

        if (!state.IsWindowMaximized)
        {
            state.WindowWidth = window.Width;
            state.WindowHeight = window.Height;
        }

        state.IsInitialized = true;
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        this.Log().Info("Shutting down the settings app");

        this.mutex?.ReleaseMutex();
        this.mutex?.Dispose();
    }

    private Subject<Unit> ConfigureSingleInstance()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName()?.Name ?? String.Empty;

        this.mutex = AppLocator.Current
            .GetRequiredService<ISingleInstanceService>()
            .TryAcquireMutex(assemblyName);

        var namedPipeService = AppLocator.Current.GetRequiredService<INamedPipeService>();

        namedPipeService.StartServer(assemblyName);

        var openExternally = new Subject<Unit>();

        namedPipeService.ReceivedString
            .Discard()
            .Subscribe(openExternally);

        return openExternally;
    }
}
