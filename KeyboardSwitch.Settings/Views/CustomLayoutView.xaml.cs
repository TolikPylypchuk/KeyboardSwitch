using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

namespace KeyboardSwitch.Settings.Views
{
    public class CustomLayoutView : ReactiveUserControl<CustomLayoutViewModel>
    {
        public CustomLayoutView()
            => this.InitializeComponent();

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
