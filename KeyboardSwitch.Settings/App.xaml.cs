using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Settings.Core.State;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Views;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings
{
    public class App : Application, IEnableLogger
    {
        private IClassicDesktopStyleApplicationLifetime desktop = null!;
        private readonly Subject<Unit> openExternally = new Subject<Unit>();

        public IDisposable OnAppExitDisposable { get; set; } = null!;

        public IObserver<Unit> OpenExternally
            => this.openExternally.AsObserver();

        public override void Initialize()
            => AvaloniaXamlLoader.Load(this);

        public override async void OnFrameworkInitializationCompleted()
        {
            this.Log().Info("Starting the settings app");

            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                this.desktop = desktop;

                desktop.Exit += this.OnExit;

                var autoSuspendHelper = new AutoSuspendHelper(desktop);
                GC.KeepAlive(autoSuspendHelper);

                RxApp.SuspensionHost.CreateNewAppState = () => new AppState();
                RxApp.SuspensionHost.SetupDefaultSuspendResume();

                autoSuspendHelper.OnFrameworkInitializationCompleted();

                var settings = await Locator.Current.GetService<ISettingsService>().GetSwitchSettingsAsync();

                var mainViewModel = new MainViewModel(settings);

                this.openExternally.InvokeCommand(mainViewModel.OpenExternally);

                desktop.MainWindow = await this.CreateMainWindow(mainViewModel);
                desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
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

            Observable.FromEventPattern<AvaloniaPropertyChangedEventArgs>(window, nameof(window.PropertyChanged))
                .Select(data => data.EventArgs.Property.Name)
                .Where(name => name == nameof(window.WindowState) || name == nameof(window.ClientSize))
                .Discard()
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
            this.OnAppExitDisposable.Dispose();
        }
    }
}
