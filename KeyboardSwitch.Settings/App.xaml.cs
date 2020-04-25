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
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

                desktop.MainWindow = new MainWindow
                {
                    ViewModel = new MainViewModel(new ServiceViewModel())
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
