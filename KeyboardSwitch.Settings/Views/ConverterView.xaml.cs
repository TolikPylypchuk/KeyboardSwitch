using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

namespace KeyboardSwitch.Settings.Views
{
    public class ConverterView : ReactiveUserControl<ConverterViewModel>
    {
        public ConverterView()
            => this.InitializeComponent();

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
