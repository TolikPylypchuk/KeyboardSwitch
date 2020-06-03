using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

namespace KeyboardSwitch.Settings.Views
{
    public class ConverterSettingsView : ReactiveUserControl<ConverterSettingsViewModel>
    {
        public ConverterSettingsView()
            => this.InitializeComponent();

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
