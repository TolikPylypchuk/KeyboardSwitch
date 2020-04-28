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

        public override async void OnFrameworkInitializationCompleted()
        {
            this.Log().Info("Starting the settings app");

            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var settings = await Locator.Current.GetService<ISettingsService>().GetUISettingsAsync();

                var mainWindow = new MainWindow
                {
                    ViewModel = new MainViewModel(),
                    Width = settings.WindowWidth,
                    Height = settings.WindowHeight
                };

                if (settings.WindowX >= 0 && settings.WindowY >= 0)
                {
                    mainWindow.Position = new PixelPoint(settings.WindowX, settings.WindowY);
                }

                desktop.MainWindow = mainWindow;
                desktop.MainWindow.Show();

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
