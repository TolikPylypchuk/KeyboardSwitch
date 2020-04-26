using System;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Views;

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
            this.Log().Info("Starting the settings app");

            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow
                {
                    ViewModel = new MainViewModel()
                };

                desktop.MainWindow = mainWindow;

                desktop.Exit += this.OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            this.Log().Info("Shutting down the settings app");
            this.OnAppExitDisposable.Dispose();
        }
    }
}
