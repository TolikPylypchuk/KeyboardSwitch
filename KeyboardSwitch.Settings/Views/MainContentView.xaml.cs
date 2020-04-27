using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

namespace KeyboardSwitch.Settings.Views
{
    public class MainContentView : ReactiveUserControl<MainContentViewModel>
    {
        public MainContentView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
