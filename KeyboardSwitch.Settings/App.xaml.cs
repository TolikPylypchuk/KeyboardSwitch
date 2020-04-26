using System;
using System.Reflection;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Views;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings
{
    public class App : Application, IEnableLogger
    {
        public IDisposable OnAppExitDisposable { get; set; } = null!;

        public override void Initialize()
            => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                this.InitializeDesktop(desktop);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void InitializeDesktop(IClassicDesktopStyleApplicationLifetime desktop)
        {
            this.Log().Info("Starting the settings app");

            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            var mainViewModel = new MainViewModel();

            desktop.MainWindow = new MainWindow { ViewModel = mainViewModel };

            desktop.Exit += this.OnExit;
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            this.Log().Info("Shutting down the settings app");
            this.OnAppExitDisposable.Dispose();
        }
    }
}
