using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
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

        public IDisposable OnAppExitDisposable { get; set; } = null!;

        public override void Initialize()
            => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            this.Log().Info("Starting the settings app");

            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                this.desktop = desktop;

                var autoSuspendHelper = new AutoSuspendHelper(desktop);
                GC.KeepAlive(autoSuspendHelper);

                RxApp.SuspensionHost.CreateNewAppState = () => new AppState();
                RxApp.SuspensionHost.SetupDefaultSuspendResume();

                autoSuspendHelper.OnFrameworkInitializationCompleted();

                desktop.MainWindow = this.CreateMainWindow(new MainViewModel());

                desktop.Exit += this.OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private MainWindow CreateMainWindow(MainViewModel viewModel)
        {
            var state = RxApp.SuspensionHost.GetAppState<AppState>();

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
